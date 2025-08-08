using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowService.Domain.Entities;
using WorkflowService.Infrastructure.Data;
using Shared.Kernel.Events;
using Shared.Kernel.Services;
using FluentValidation;

namespace WorkflowService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowInstancesController : ControllerBase
{
    private readonly WorkflowDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<StartWorkflowRequest> _startValidator;
    private readonly ILogger<WorkflowInstancesController> _logger;

    public WorkflowInstancesController(
        WorkflowDbContext context,
        IEventPublisher eventPublisher,
        IValidator<StartWorkflowRequest> startValidator,
        ILogger<WorkflowInstancesController> logger)
    {
        _context = context;
        _eventPublisher = eventPublisher;
        _startValidator = startValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowInstanceDto>>> GetWorkflowInstances(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? workflowDefinitionId = null)
    {
        var query = _context.WorkflowInstances
            .Include(wi => wi.WorkflowDefinition)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(wi => wi.Status == status);
        }

        if (workflowDefinitionId.HasValue)
        {
            query = query.Where(wi => wi.WorkflowDefinitionId == workflowDefinitionId.Value);
        }

        var totalCount = await query.CountAsync();
        var instances = await query
            .OrderByDescending(wi => wi.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(wi => new WorkflowInstanceDto
            {
                Id = wi.Id,
                WorkflowDefinitionId = wi.WorkflowDefinitionId,
                WorkflowDefinitionName = wi.WorkflowDefinition.Name,
                Status = wi.Status,
                StartedAt = wi.StartedAt,
                CompletedAt = wi.CompletedAt,
                CreatedAt = wi.CreatedAt,
                UpdatedAt = wi.UpdatedAt
            })
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        return Ok(instances);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowInstanceDetailDto>> GetWorkflowInstance(Guid id)
    {
        var instance = await _context.WorkflowInstances
            .Include(wi => wi.WorkflowDefinition)
            .Include(wi => wi.Executions)
            .FirstOrDefaultAsync(wi => wi.Id == id);

        if (instance == null)
        {
            return NotFound();
        }

        var dto = new WorkflowInstanceDetailDto
        {
            Id = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowDefinitionName = instance.WorkflowDefinition.Name,
            Status = instance.Status,
            Input = instance.Input,
            Output = instance.Output,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            Executions = instance.Executions.Select(e => new WorkflowExecutionDto
            {
                Id = e.Id,
                StepName = e.StepName,
                Status = e.Status,
                Input = e.Input,
                Output = e.Output,
                StartedAt = e.StartedAt,
                CompletedAt = e.CompletedAt,
                ErrorMessage = e.ErrorMessage
            }).OrderBy(e => e.StartedAt).ToList(),
            CreatedAt = instance.CreatedAt,
            UpdatedAt = instance.UpdatedAt
        };

        return Ok(dto);
    }

    [HttpPost("start")]
    public async Task<ActionResult<WorkflowInstanceDto>> StartWorkflow(StartWorkflowRequest request)
    {
        var validationResult = await _startValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var workflowDefinition = await _context.WorkflowDefinitions
            .FirstOrDefaultAsync(wd => wd.Id == request.WorkflowDefinitionId && wd.IsActive);

        if (workflowDefinition == null)
        {
            return BadRequest("Workflow definition not found or not active");
        }

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = request.WorkflowDefinitionId,
            Status = "Running",
            Input = request.Input ?? "{}",
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync();

        // Publish workflow started event
        await _eventPublisher.PublishAsync(new WorkflowStartedEvent
        {
            WorkflowInstanceId = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowDefinitionName = workflowDefinition.Name,
            Input = instance.Input,
            StartedAt = instance.StartedAt,
            UserId = User.Identity?.Name
        });

        _logger.LogInformation("Workflow started: {WorkflowInstanceId} from definition {WorkflowDefinitionId} by user {UserId}",
            instance.Id, request.WorkflowDefinitionId, User.Identity?.Name);

        var dto = new WorkflowInstanceDto
        {
            Id = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowDefinitionName = workflowDefinition.Name,
            Status = instance.Status,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            CreatedAt = instance.CreatedAt,
            UpdatedAt = instance.UpdatedAt
        };

        return CreatedAtAction(nameof(GetWorkflowInstance), new { id = instance.Id }, dto);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelWorkflow(Guid id)
    {
        var instance = await _context.WorkflowInstances.FindAsync(id);
        if (instance == null)
        {
            return NotFound();
        }

        if (instance.Status != "Running")
        {
            return BadRequest("Can only cancel running workflows");
        }

        instance.Status = "Cancelled";
        instance.CompletedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish workflow cancelled event
        await _eventPublisher.PublishAsync(new WorkflowCancelledEvent
        {
            WorkflowInstanceId = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            CancelledAt = instance.CompletedAt.Value,
            UserId = User.Identity?.Name
        });

        _logger.LogInformation("Workflow cancelled: {WorkflowInstanceId} by user {UserId}", 
            instance.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,WorkflowManager,System")]
    public async Task<IActionResult> CompleteWorkflow(Guid id, [FromBody] CompleteWorkflowRequest request)
    {
        var instance = await _context.WorkflowInstances.FindAsync(id);
        if (instance == null)
        {
            return NotFound();
        }

        if (instance.Status != "Running")
        {
            return BadRequest("Can only complete running workflows");
        }

        instance.Status = request.Status;
        instance.Output = request.Output;
        instance.CompletedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish workflow completed event
        await _eventPublisher.PublishAsync(new WorkflowCompletedEvent
        {
            WorkflowInstanceId = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            Status = instance.Status,
            Output = instance.Output,
            CompletedAt = instance.CompletedAt.Value,
            Duration = instance.CompletedAt.Value - instance.StartedAt
        });

        _logger.LogInformation("Workflow completed: {WorkflowInstanceId} with status {Status}",
            instance.Id, instance.Status);

        return NoContent();
    }
}

// DTOs
public class WorkflowInstanceDto
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string WorkflowDefinitionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WorkflowInstanceDetailDto : WorkflowInstanceDto
{
    public string Input { get; set; } = string.Empty;
    public string? Output { get; set; }
    public List<WorkflowExecutionDto> Executions { get; set; } = new();
}

public class WorkflowExecutionDto
{
    public Guid Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string? Output { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

// Request models
public class StartWorkflowRequest
{
    public Guid WorkflowDefinitionId { get; set; }
    public string? Input { get; set; }
}

public class CompleteWorkflowRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Output { get; set; }
}

// Events
public class WorkflowStartedEvent : IDomainEvent
{
    public Guid WorkflowInstanceId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string WorkflowDefinitionName { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public string? UserId { get; set; }
}

public class WorkflowCancelledEvent : IDomainEvent
{
    public Guid WorkflowInstanceId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public DateTime CancelledAt { get; set; }
    public string? UserId { get; set; }
}

// Validators
public class StartWorkflowRequestValidator : AbstractValidator<StartWorkflowRequest>
{
    public StartWorkflowRequestValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId)
            .NotEmpty();

        RuleFor(x => x.Input)
            .Must(BeValidJsonOrNull)
            .WithMessage("Input must be valid JSON");
    }

    private bool BeValidJsonOrNull(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
