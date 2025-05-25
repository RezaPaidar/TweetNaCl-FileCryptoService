namespace FileCryptoService.NaCl
{
    public class KeyPair
    {
        public Byte[] PublicKey { get; protected set; }
        public Byte[] SecretKey { get; protected set; }

        public KeyPair(Byte[] publicKey, Byte[] secretKey)
        {
            this.PublicKey = publicKey;
            this.SecretKey = secretKey;
        }
    }
}
