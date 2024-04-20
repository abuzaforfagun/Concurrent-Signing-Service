using KeyManagement.Api.Models;
using KeyManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeyManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class KeysController : ControllerBase
{
    private readonly IKeyStorageService _keyStorage;

    public KeysController(IKeyStorageService keyStorage)
    {
        _keyStorage = keyStorage;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetPublicKeyOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var (id, publicKey) = await _keyStorage.PopLeastUsedKeyAsync();

        if(id is null) return NotFound();

        return Ok(new GetPublicKeyOutput
        {
            Id = id.Value,
            PublicKey = publicKey!
        });
    }

    [HttpPost]
    [Route("release-lock")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReleaseLock(Guid id)
    {
        var isSuccess = await _keyStorage.ReleaseLockAsync(id);
        if (!isSuccess)
        {
            return BadRequest();
        }

        return Accepted();
    }
}