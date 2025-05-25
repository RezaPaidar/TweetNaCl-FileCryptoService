namespace FileCryptoService.NaCl
{
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.IO;

    public class FileEncryptorService
    {
        private const int ChunkSize = 4096 * 16;
        private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
        private byte[] _encryptionBuffer;
        private byte[] _lengthBuffer = new byte[4];

        public FileEncryptorService()
        {
            _encryptionBuffer = _arrayPool.Rent(ChunkSize);
        }

        public async Task<(byte[] nonce, string outputPath)> EncryptFileAsync(string inputFilePath, string outputFilePath, byte[] publicKey, byte[] secretKey)
        {
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Input file not found", inputFilePath);

            var nonce = new byte[TweetNaCl.BoxNonceBytes];
            TweetNaCl.RandomBytes(nonce);

            await using (var inputStream = new FileStream(
                inputFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: ChunkSize,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var outputStream = new FileStream(
                outputFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: ChunkSize,
                options: FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await outputStream.WriteAsync(nonce.AsMemory(0, nonce.Length));

                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(_encryptionBuffer.AsMemory(0, ChunkSize))) > 0)
                {
                    var dataToEncrypt = _encryptionBuffer.AsSpan(0, bytesRead).ToArray();
                    var encryptedChunk = TweetNaCl.CryptoBox(dataToEncrypt, nonce, publicKey, secretKey);

                    BinaryPrimitives.WriteInt32LittleEndian(_lengthBuffer.AsSpan(), encryptedChunk.Length);
                    await outputStream.WriteAsync(_lengthBuffer.AsMemory(0, 4));
                    await outputStream.WriteAsync(encryptedChunk.AsMemory(0, encryptedChunk.Length));
                }
            }

            return (nonce, outputFilePath);
        }
        public async Task DecryptFileAsync(string inputFilePath, string outputFilePath, byte[] publicKey, byte[] secretKey)
        {
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Input file not found", inputFilePath);

            await using var inputStream = new FileStream(
                inputFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: ChunkSize,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using var outputStream = new FileStream(
                outputFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: ChunkSize,
                options: FileOptions.Asynchronous | FileOptions.WriteThrough);

            var nonce = new byte[TweetNaCl.BoxNonceBytes];
            if (await inputStream.ReadAsync(nonce, 0, nonce.Length) != nonce.Length)
                throw new InvalidDataException("File is too short to contain nonce");

            byte[] lengthBuffer = new byte[4];
            byte[] encryptedChunkBuffer = null;

            try
            {
                while (inputStream.Position < inputStream.Length)
                {
                    if (await inputStream.ReadAsync(lengthBuffer, 0, 4) != 4)
                        throw new InvalidDataException("Unexpected end of file while reading chunk length");

                    int chunkLength = BitConverter.ToInt32(lengthBuffer, 0);

                    if (encryptedChunkBuffer == null || encryptedChunkBuffer.Length < chunkLength)
                    {
                        if (encryptedChunkBuffer != null)
                            ArrayPool<byte>.Shared.Return(encryptedChunkBuffer);
                        encryptedChunkBuffer = ArrayPool<byte>.Shared.Rent(chunkLength);
                    }

                    if (await inputStream.ReadAsync(encryptedChunkBuffer, 0, chunkLength) != chunkLength)
                        throw new InvalidDataException("Unexpected end of file while reading encrypted chunk");

                    var encryptedChunk = new byte[chunkLength];
                    Buffer.BlockCopy(encryptedChunkBuffer, 0, encryptedChunk, 0, chunkLength);
                    var decryptedChunk = TweetNaCl.CryptoBoxOpen(encryptedChunk, nonce, publicKey, secretKey);

                    await outputStream.WriteAsync(decryptedChunk, 0, decryptedChunk.Length);
                }
            }
            finally
            {
                if (encryptedChunkBuffer != null)
                    ArrayPool<byte>.Shared.Return(encryptedChunkBuffer);
            }
        }
        public KeyPair GenerateKeyPair()
        {
            return TweetNaCl.CryptoBoxKeypair();
        }
        public async Task<(KeyPair keyPair, byte[] nonce, string outputPath)> EncryptFileWithNewKeyAsync(string inputFilePath, string outputFilePath)
        {
            var keyPair = GenerateKeyPair();
            var result = await EncryptFileAsync(inputFilePath, outputFilePath, keyPair.PublicKey, keyPair.SecretKey);
            return (keyPair, result.nonce, result.outputPath);
        }
        public void Dispose()
        {
            if (_encryptionBuffer != null)
            {
                _arrayPool.Return(_encryptionBuffer);
                _encryptionBuffer = null;
            }
        }
    }
}
