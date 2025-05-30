namespace FileCryptoService.ViewModels
{
    public class DecryptRequest
    {
        public string Base64Data { get; set; }
        public string SecretKey { get; set; }
    }
}
