using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SigningService.Api.Models;

namespace SigningService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SigningController : ControllerBase
{
    [HttpPost]
    [Route("sign")]
    [ProducesResponseType(typeof(List<SigningOutput>), StatusCodes.Status200OK)]
    public IActionResult Sign(SigningInput input)
    {
        var result = new List<SigningOutput>();

        foreach (var data in input.Data)
        {
            byte[] bytesToSign = Encoding.UTF8.GetBytes(data.Content);

            using var rsa = new RSACryptoServiceProvider();
            var plainPublicKey = Encoding.UTF8.GetString(Convert.FromBase64String(input.PublicKey));
            rsa.FromXmlString(plainPublicKey);

            byte[] signature = rsa.Encrypt(bytesToSign, false);

            result.Add(new SigningOutput
            {
                Id = data.Id,
                SignedData = Convert.ToBase64String(signature)
            });
        }

        return Ok(result);
    }
}