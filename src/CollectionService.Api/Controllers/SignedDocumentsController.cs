using CollectionService.Api.Dtos;
using CollectionService.Api.Models;
using CollectionService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollectionService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SignedDocumentsController : ControllerBase
{
    private readonly IDocumentCollectionService _documentCollectionService;

    public SignedDocumentsController(IDocumentCollectionService documentCollectionService)
    {
        _documentCollectionService = documentCollectionService;
    }

    [HttpPost]
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