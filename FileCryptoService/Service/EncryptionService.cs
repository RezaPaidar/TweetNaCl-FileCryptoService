using System.Text.Json;
using FileCryptoService.NaCl;
using FileCryptoService.ViewModels;

namespace FileCryptoService.Service;

public interface ICryptoService
{
    Task<CryptoResult> EncryptFileAsync(EncryptRequest request);
    Task<CryptoResult> DecryptFileAsync(DecryptRequest request);
    CryptoResult GenerateKeys();
}

public class CryptoService : ICryptoService
{
    private readonly FileEncryptorService _fileEncryptor;
    public CryptoService()
    {
        _fileEncryptor = new FileEncryptorService();
    }

    public async Task<CryptoResult> EncryptFileAsync(EncryptRequest request)
    {
        try
        {
            var recipientPublicKey = Convert.FromBase64String(request.PublicKey);
            if (recipientPublicKey.Length != TweetNaCl.BoxPublicKeyBytes)
                return new CryptoResult { Success = false, Message = "Invalid public key length" };

            using var stream = new MemoryStream();
            await request.File.CopyToAsync(stream);
            var fileData = stream.ToArray();

            var ephemeralKeyPair = TweetNaCl.CryptoBoxKeypair();

            var nonce = new byte[TweetNaCl.BoxNonceBytes];
            TweetNaCl.RandomBytes(nonce);

            var ciphertext = TweetNaCl.CryptoBox(fileData, nonce, recipientPublicKey, ephemeralKeyPair.SecretKey);

            var output = new byte[ephemeralKeyPair.PublicKey.Length + nonce.Length + ciphertext.Length];
            Buffer.BlockCopy(ephemeralKeyPair.PublicKey, 0, output, 0, ephemeralKeyPair.PublicKey.Length);
            Buffer.BlockCopy(nonce, 0, output, ephemeralKeyPair.PublicKey.Length, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, output, ephemeralKeyPair.PublicKey.Length + nonce.Length, ciphertext.Length);

            return new CryptoResult
            {
                Success = true,
                EncryptedData = Convert.ToBase64String(output)
            };
        }
        catch (Exception ex)
        {
            return new CryptoResult
            {
                Success = false,
                Message = $"Encryption failed: {ex.Message}"
            };
        }
        finally
        {
            _fileEncryptor.Dispose();
        }
    }

    public async Task<CryptoResult> DecryptFileAsync(DecryptRequest request)
    {
        try
        {
            var secretKey = Convert.FromBase64String(request.SecretKey);
            if (secretKey.Length != TweetNaCl.BoxSecretKeyBytes)
                return new CryptoResult { Success = false, Message = "Invalid secret key length" };

            var encryptedData = Convert.FromBase64String(request.Base64Data);

            if (encryptedData.Length < TweetNaCl.BoxPublicKeyBytes + TweetNaCl.BoxNonceBytes)
                return new CryptoResult { Success = false, Message = "Invalid encrypted data" };

            var ephemeralPubKey = new byte[TweetNaCl.BoxPublicKeyBytes];
            var nonce = new byte[TweetNaCl.BoxNonceBytes];
            var ciphertext = new byte[encryptedData.Length - (ephemeralPubKey.Length + nonce.Length)];

            Buffer.BlockCopy(encryptedData, 0, ephemeralPubKey, 0, ephemeralPubKey.Length);
            Buffer.BlockCopy(encryptedData, ephemeralPubKey.Length, nonce, 0, nonce.Length);
            Buffer.BlockCopy(encryptedData, ephemeralPubKey.Length + nonce.Length, ciphertext, 0, ciphertext.Length);

            var plaintext = TweetNaCl.CryptoBoxOpen(ciphertext, nonce, ephemeralPubKey, secretKey);

            return new CryptoResult
            {
                Success = true,
                DecryptedData = plaintext
            };
        }
        catch (Exception ex)
        {
            return new CryptoResult
            {
                Success = false,
                Message = $"Decryption failed: {ex.Message}"
            };
        }
        finally
        {
            _fileEncryptor.Dispose();
        }
    }

    public CryptoResult GenerateKeys()
    {
        try
        {
            var keyPair = TweetNaCl.CryptoBoxKeypair();

            return new CryptoResult
            {
                Success = true,
                PublicKey = Convert.ToBase64String(keyPair.PublicKey),
                SecretKey = Convert.ToBase64String(keyPair.SecretKey)
            };
        }
        catch (Exception ex)
        {
            return new CryptoResult
            {
                Success = false,
                Message = $"Key generation failed: {ex.Message}"
            };
        }
    }
}