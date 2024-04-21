using System.Security.Cryptography;
using System.Text;
using KeyManagement.Api.Client;
using Microsoft.AspNetCore.Mvc;
using SigningService.Api.Models;

namespace SigningService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SigningController : ControllerBase
{
    private readonly IKeysClient _keysClient;

    public SigningController(IKeysClient keysClient)
    {
        _keysClient = keysClient;
    }

    [HttpPost]
    [Route("sign")]
    [ProducesResponseType(typeof(List<SigningOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sign(SigningInput input)
    {
        var privateKey = await _keysClient.GetDetailsAsync(input.KeyId, "1");

        var result = new List<SigningOutput>();

        foreach (var data in input.Data)
        {
            byte[] bytesToSign = Encoding.UTF8.GetBytes(data.Content);

            using var rsa = new RSACryptoServiceProvider();
            var plainPrivateKey = Encoding.UTF8.GetString(Convert.FromBase64String(privateKey.PrivateKey));
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