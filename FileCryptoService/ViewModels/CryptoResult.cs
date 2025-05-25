namespace FileCryptoService.ViewModels
{
    public class CryptoResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Data { get; set; } // Base64 encoded result
    }
}
