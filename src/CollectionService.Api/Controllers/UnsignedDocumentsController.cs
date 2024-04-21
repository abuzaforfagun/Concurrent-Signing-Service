﻿using CollectionService.Api.Attributes;
using CollectionService.Api.Dtos;
using CollectionService.Api.Models;
using CollectionService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollectionService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UnsignedDocumentsController : ControllerBase
{
    private readonly IDocumentCollectionService _documentCollectionService;

    public UnsignedDocumentsController(IDocumentCollectionService documentCollectionService)
    {
        _documentCollectionService = documentCollectionService;
    }

    [ProducesResponseType(typeof(GetDocumentOutput<Document>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> Get(int pageNumber, [MaxNumber(1000)]int pageSize)
    {
        var documents = await _documentCollectionService.GetAsync(pageSize, (pageNumber -1)*pageSize);

        var result = new GetDocumentOutput<Document>(documents, pageNumber, pageSize);

        return Ok(result);
    }
}