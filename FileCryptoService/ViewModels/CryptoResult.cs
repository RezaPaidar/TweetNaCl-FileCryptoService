namespace FileCryptoService.ViewModels
{
    public class CryptoResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string EncryptedData { get; set; }
        public byte[] DecryptedData { get; set; }
    }
}
