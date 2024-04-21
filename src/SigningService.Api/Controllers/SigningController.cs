using System.Security.Cryptography;
using System.Text;
using ConcurrentSigning.Cryptography;
using KeyManagement.Api.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SigningService.Api.Models;

namespace SigningService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SigningController : ControllerBase
{
    private readonly IKeysClient _keysClient;
    private readonly EncryptionOptions _encryptionOptions;

    public SigningController(IKeysClient keysClient, IOptions<EncryptionOptions> encryptionOptions)
    {
        _keysClient = keysClient;
        _encryptionOptions = encryptionOptions.Value;
    }

    [HttpPost]
    [Route("sign")]
    [ProducesResponseType(typeof(List<SigningOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sign(SigningInput input)
    {
        var privateKeyPayload = await _keysClient.GetDetailsAsync(input.KeyId);
        var plainPrivateKey =
            Encoding.UTF8.GetString(Convert.FromBase64String(SymmetricEncryption.Decrypt(privateKeyPayload.PrivateKey,
                _encryptionOptions.PrivateKey)));

        var result = new List<SigningOutput>();

        foreach (var data in input.Data)
        {
            byte[] bytesToSign = Encoding.UTF8.GetBytes(data.Content);

            using var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(plainPrivateKey);

            byte[] signature = rsa.SignData(bytesToSign, "SHA256");

            result.Add(new SigningOutput
            {
                Id = data.Id,
                SignedData = Convert.ToBase64String(signature)
            });
        }

        return Ok(result);
    }
}