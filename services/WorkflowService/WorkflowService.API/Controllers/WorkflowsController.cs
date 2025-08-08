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
public class WorkflowsController : ControllerBase
{
    private readonly WorkflowDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<CreateWorkflowRequest> _createValidator;
    private readonly IValidator<UpdateWorkflowRequest> _updateValidator;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(
        WorkflowDbContext context,
        IEventPublisher eventPublisher,
        IValidator<CreateWorkflowRequest> createValidator,
        IValidator<UpdateWorkflowRequest> updateValidator,
        ILogger<WorkflowsController> logger)
    {
        _context = context;
        _eventPublisher = eventPublisher;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDefinitionDto>>> GetWorkflows(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var query = _context.WorkflowDefinitions.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(w => w.Name.Contains(search) || w.Description.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var workflows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkflowDefinitionDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                Version = w.Version,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        return Ok(workflows);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDefinitionDetailDto>> GetWorkflow(Guid id)
    {
        var workflow = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
        {
            return NotFound();
        }

        var dto = new WorkflowDefinitionDetailDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Description = workflow.Description,
            Version = workflow.Version,
            IsActive = workflow.IsActive,
            WorkflowJson = workflow.WorkflowJson,
            Steps = workflow.Steps.Select(s => new WorkflowStepDto
            {
                Id = s.Id,
                Name = s.Name,
                StepType = s.StepType,
                Configuration = s.Configuration,
                Order = s.Order
            }).ToList(),
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt
        };

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,WorkflowManager")]
    public async Task<ActionResult<WorkflowDefinitionDto>> CreateWorkflow(CreateWorkflowRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var workflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Version = 1,
            IsActive = false,
            WorkflowJson = request.WorkflowJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add steps if provided
        if (request.Steps?.Any() == true)
        {
            foreach (var stepRequest in request.Steps)
            {
                workflow.Steps.Add(new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    WorkflowDefinitionId = workflow.Id,
                    Name = stepRequest.Name,
                    StepType = stepRequest.StepType,
                    Configuration = stepRequest.Configuration,
                    Order = stepRequest.Order
                });
            }
        }

        _context.WorkflowDefinitions.Add(workflow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Workflow created: {WorkflowId} by user {UserId}", workflow.Id, User.Identity?.Name);

        var dto = new WorkflowDefinitionDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Description = workflow.Description,
            Version = workflow.Version,
            IsActive = workflow.IsActive,
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt
        };

        return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,WorkflowManager")]
    public async Task<IActionResult> UpdateWorkflow(Guid id, UpdateWorkflowRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var workflow = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
        {
            return NotFound();
        }

        workflow.Name = request.Name;
        workflow.Description = request.Description;
        workflow.WorkflowJson = request.WorkflowJson;
        workflow.UpdatedAt = DateTime.UtcNow;

        // Update steps
        if (request.Steps?.Any() == true)
        {
            // Remove existing steps
            _context.WorkflowSteps.RemoveRange(workflow.Steps);

            // Add new steps
            foreach (var stepRequest in request.Steps)
            {
                workflow.Steps.Add(new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    WorkflowDefinitionId = workflow.Id,
                    Name = stepRequest.Name,
                    StepType = stepRequest.StepType,
                    Configuration = stepRequest.Configuration,
                    Order = stepRequest.Order
                });
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Workflow updated: {WorkflowId} by user {UserId}", workflow.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpPost("{id}/activate")]
    [Authorize(Roles = "Admin,WorkflowManager")]
    public async Task<IActionResult> ActivateWorkflow(Guid id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        workflow.IsActive = true;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Workflow activated: {WorkflowId} by user {UserId}", workflow.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Admin,WorkflowManager")]
    public async Task<IActionResult> DeactivateWorkflow(Guid id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        workflow.IsActive = false;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Workflow deactivated: {WorkflowId} by user {UserId}", workflow.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteWorkflow(Guid id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        // Check if workflow has active instances
        var hasActiveInstances = await _context.WorkflowInstances
            .AnyAsync(wi => wi.WorkflowDefinitionId == id && wi.Status == "Running");

        if (hasActiveInstances)
        {
            return BadRequest("Cannot delete workflow with active instances");
        }

        _context.WorkflowDefinitions.Remove(workflow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Workflow deleted: {WorkflowId} by user {UserId}", workflow.Id, User.Identity?.Name);

        return NoContent();
    }
}

// DTOs
public class WorkflowDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WorkflowDefinitionDetailDto : WorkflowDefinitionDto
{
    public string WorkflowJson { get; set; } = string.Empty;
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public int Order { get; set; }
}

// Request models
public class CreateWorkflowRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowJson { get; set; } = string.Empty;
    public List<CreateWorkflowStepRequest>? Steps { get; set; }
}

public class UpdateWorkflowRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowJson { get; set; } = string.Empty;
    public List<CreateWorkflowStepRequest>? Steps { get; set; }
}

public class CreateWorkflowStepRequest
{
    public string Name { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public int Order { get; set; }
}

// Validators
public class CreateWorkflowRequestValidator : AbstractValidator<CreateWorkflowRequest>
{
    public CreateWorkflowRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.WorkflowJson)
            .NotEmpty()
            .Must(BeValidJson)
            .WithMessage("Invalid JSON format");
    }

    private bool BeValidJson(string json)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class UpdateWorkflowRequestValidator : AbstractValidator<UpdateWorkflowRequest>
{
    public UpdateWorkflowRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.WorkflowJson)
            .NotEmpty()
            .Must(BeValidJson)
            .WithMessage("Invalid JSON format");
    }

    private bool BeValidJson(string json)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
