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
    public async Task<IActionResult> Get()
    {
        var (id, publicKey) = await _keyStorage.PopLeastUsedKeyAsync();

        return Ok(new GetPublicKeyOutput
        {
            Id = id,
            PublicKey = publicKey
        });
    }

    [HttpPost]
    [Route("release-lock")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ReleaseLock(Guid id)
    {
        await _keyStorage.ReleaseLockAsync(id);

        return Accepted();
    }
}