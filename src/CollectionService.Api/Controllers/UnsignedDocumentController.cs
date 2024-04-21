using CollectionService.Api.Attributes;
using CollectionService.Api.Dtos;
using CollectionService.Api.Models;
using CollectionService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollectionService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UnsignedDocumentController : ControllerBase
{
    private readonly IDocumentCollectionService _documentCollectionService;

    public UnsignedDocumentController(IDocumentCollectionService documentCollectionService)
    {
        _documentCollectionService = documentCollectionService;
    }

    [ProducesResponseType(typeof(GetDocumentOutput<UnsignedDocument>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> Get(int pageNumber, [MaxNumber(1000)]int pageSize)
    {
        var documents = await _documentCollectionService.GetAsync(pageSize, (pageNumber -1)*pageSize);

        var result = new GetDocumentOutput<UnsignedDocument>(documents, pageNumber, pageSize);

        return Ok(result);
    }
}