using Asp.Versioning;
using CollectionService.Api.Attributes;
using CollectionService.Api.Dtos;
using CollectionService.Api.Models;
using CollectionService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollectionService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/documents")]
[ApiVersion("1")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentCollectionService _documentCollectionService;

    public DocumentsController(IDocumentCollectionService documentCollectionService)
    {
        _documentCollectionService = documentCollectionService;
    }

    [ProducesResponseType(typeof(GetDocumentOutput<UnSignedDocument>), StatusCodes.Status200OK)]
    [HttpGet("unsigned")]
    public async Task<IActionResult> GetAllUnsigned(int pageNumber, [MaxNumber(1000)] int pageSize)
    {
        var numberOfDocuments = await _documentCollectionService.GetNumberOfUnSignedDocuments();
        var documents = await _documentCollectionService.GetAsync(pageSize, (pageNumber - 1) * pageSize);

        var result = new GetDocumentOutput<UnSignedDocument>(documents, pageNumber, pageSize, numberOfDocuments);

        return Ok(result);
    }

    [HttpPost("signed")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSigned(List<AddSignedDocumentInput> input)
    {
        if (input.Count == 0)
        {
            return BadRequest();
        }

        var payLoad = input.Select(d => new SignedDocument
        {
            Content = d.Content,
            DocumentId = d.DocumentId
        }).ToList();

        var result = await _documentCollectionService.AddSignedDocumentAsync(payLoad);

        if (!result.IsSuccess)
        {
            return BadRequest();
        }

        return Accepted();
    }
}