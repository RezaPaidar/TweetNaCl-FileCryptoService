namespace FileCryptoService.ViewModels
{
    public class EncryptRequest
    {
        public IFormFile File { get; set; } = default!;
        public string PublicKey { get; set; } 
        public string SecretKey { get; set; } 
    }
}
