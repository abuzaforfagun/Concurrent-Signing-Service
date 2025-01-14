﻿using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using ConcurrentSigning.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SigningService.Api.Models;

namespace SigningService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/keys")]
[ApiVersion("1")]
public class SigningController : ControllerBase
{
    private readonly EncryptionOptions _encryptionOptions;

    public SigningController(IOptions<EncryptionOptions> encryptionOptions)
    {
        _encryptionOptions = encryptionOptions.Value;
    }

    [HttpPost]
    [Route("sign")]
    [ProducesResponseType(typeof(List<SigningOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sign(SigningInput input)
    {
        var plainPrivateKey =
            Encoding.UTF8.GetString(Convert.FromBase64String(SymmetricEncryption.Decrypt(input.PrivateKey,
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