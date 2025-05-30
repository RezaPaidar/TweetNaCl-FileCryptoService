using FileCryptoService.Service;
using FileCryptoService.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FileCryptoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileCryptoController : ControllerBase
    {
        private readonly ICryptoService _cryptoService;

        public FileCryptoController(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        [HttpGet("generate-keys")]
        public ActionResult<CryptoResult> GenerateKeys()
        {
            var result = _cryptoService.GenerateKeys();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("encrypt-file")]
        public async Task<ActionResult<CryptoResult>> EncryptFile([FromForm] EncryptRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded");

            if (string.IsNullOrEmpty(request.PublicKey))
                return BadRequest("The public key is required");

            var result = await _cryptoService.EncryptFileAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("decrypt-file")]
        public async Task<ActionResult<CryptoResult>> DecryptFile([FromBody] DecryptRequest request)
        {
            if (string.IsNullOrEmpty(request.Base64Data))
                return BadRequest("No data provided");

            if (string.IsNullOrEmpty(request.SecretKey))
                return BadRequest("The secret key is required");

            var result = await _cryptoService.DecryptFileAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}

