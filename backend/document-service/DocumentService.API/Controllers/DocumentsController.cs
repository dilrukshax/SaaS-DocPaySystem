using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentService.Application.Commands;
using DocumentService.Application.DTOs;
using DocumentService.Application.Queries;

namespace DocumentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upload a new document
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<DocumentDto>> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] Guid tenantId,
        [FromForm] Guid userId,
        [FromForm] string[]? tags)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(
            name,
            description,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            tenantId,
            userId,
            tags?.ToList() ?? new List<string>());

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDocument), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get documents with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<DocumentDto>>> GetDocuments(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? status = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetDocumentsQuery(
            tenantId, userId, status, tags?.ToList(), searchTerm, page, pageSize);

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
    {
        var query = new GetDocumentQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Update document metadata
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DocumentDto>> UpdateDocument(
        Guid id, 
        [FromBody] UpdateDocumentRequest request)
    {
        var command = new UpdateDocumentCommand(
            id, request.Name, request.Description, request.Tags, request.UpdatedBy);

        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Delete a document (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id, [FromQuery] Guid deletedBy)
    {
        var command = new DeleteDocumentCommand(id, deletedBy);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Add a new version to an existing document
    /// </summary>
    [HttpPost("{id}/versions")]
    public async Task<ActionResult<DocumentVersionDto>> AddDocumentVersion(
        Guid id,
        [FromForm] IFormFile file,
        [FromForm] string? comment,
        [FromForm] Guid createdBy)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        using var stream = file.OpenReadStream();
        var command = new AddDocumentVersionCommand(
            id, file.FileName, file.ContentType, file.Length, stream, comment, createdBy);

        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();

        return CreatedAtAction(nameof(GetDocumentVersions), new { documentId = id }, result);
    }

    /// <summary>
    /// Get all versions of a document
    /// </summary>
    [HttpGet("{documentId}/versions")]
    public async Task<ActionResult<IEnumerable<DocumentVersionDto>>> GetDocumentVersions(Guid documentId)
    {
        var query = new GetDocumentVersionsQuery(documentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Process OCR for a document
    /// </summary>
    [HttpPost("{id}/ocr")]
    public async Task<ActionResult<OCRResult>> ProcessOCR(
        Guid id, 
        [FromBody] ProcessOCRRequest request)
    {
        var command = new ProcessOCRCommand(id, request.Language ?? "en");
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get document download URL
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<ActionResult<DocumentUrlDto>> GetDocumentUrl(Guid id)
    {
        var query = new GetDocumentUrlQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}

public record ProcessOCRRequest(string? Language);
