using Asp.Versioning;
using ConcurrentSigning.Cryptography;
using KeyManagement.Api.Models;
using KeyManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KeyManagement.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/keys")]
[ApiVersion("1")]
public class KeysController : ControllerBase
{
    private readonly IKeyStorageService _keyStorage;
    private readonly EncryptionOptions _encryptionOptions;

    public KeysController(IKeyStorageService keyStorage, IOptions<EncryptionOptions> encryptionOptions)
    {
        _keyStorage = keyStorage;
        _encryptionOptions = encryptionOptions.Value;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GetKeyOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pop()
    {
        var key = await _keyStorage.PopLeastUsedKeyAsync();

        if(key is null) return NotFound();

        return Ok(new GetKeyOutput
        {
            Id = key.Id,
            PrivateKey = SymmetricEncryption.Encrypt(key.PrivateKey, _encryptionOptions.PrivateKey)
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetPrivateKeyOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var privateKey = await _keyStorage.GetPrivateKeyAsync(id);

        if (privateKey is null) return NotFound();

        return Ok(new GetPrivateKeyOutput()
        {
            PrivateKey = SymmetricEncryption.Encrypt(privateKey, _encryptionOptions.PrivateKey) 
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