using System.Net;
using Asp.Versioning;
using CollectionService.Api.Dtos;
using CollectionService.Api.Models;
using CollectionService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollectionService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/signed-documents")]
[ApiVersion("1")]
public class SignedDocumentsController : ControllerBase
{
    private readonly IDocumentCollectionService _documentCollectionService;

    public SignedDocumentsController(IDocumentCollectionService documentCollectionService)
    {
        _documentCollectionService = documentCollectionService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(List<AddSignedDocumentInput> input)
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