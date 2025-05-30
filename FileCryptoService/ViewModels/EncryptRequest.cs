namespace FileCryptoService.ViewModels
{
    public class EncryptRequest
    {
        public IFormFile File { get; set; }
        public string PublicKey { get; set; } 
    }
}
