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
            var publicKey = Convert.FromBase64String(request.PublicKey);
            var secretKey = Convert.FromBase64String(request.SecretKey);

            var inputPath = Path.GetTempFileName();
            var outputPath = Path.GetTempFileName();

            await using (var stream = new FileStream(inputPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var result = await _fileEncryptor.EncryptFileAsync(inputPath, outputPath, publicKey, secretKey);

            var encryptedBytes = await File.ReadAllBytesAsync(outputPath);
            var base64Result = Convert.ToBase64String(encryptedBytes);

            File.Delete(inputPath);
            File.Delete(outputPath);

            return new CryptoResult
            {
                Success = true,
                Message = "File encrypted successfully",
                Data = base64Result
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
            var publicKey = Convert.FromBase64String(request.PublicKey);
            var secretKey = Convert.FromBase64String(request.SecretKey);
            var encryptedData = Convert.FromBase64String(request.Base64Data);

            var inputPath = Path.GetTempFileName();
            var outputPath = Path.GetTempFileName();

            await File.WriteAllBytesAsync(inputPath, encryptedData);

            await _fileEncryptor.DecryptFileAsync(inputPath, outputPath, publicKey, secretKey);

            var decryptedBytes = await File.ReadAllBytesAsync(outputPath);
            var base64Result = Convert.ToBase64String(decryptedBytes);

            File.Delete(inputPath);
            File.Delete(outputPath);

            return new CryptoResult
            {
                Success = true,
                Message = "File decrypted successfully",
                Data = base64Result
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
            var keyPair = _fileEncryptor.GenerateKeyPair();

            return new CryptoResult
            {
                Success = true,
                Message = "Keys generated successfully",
                Data = JsonSerializer.Serialize(new
                {
                    PublicKey = Convert.ToBase64String(keyPair.PublicKey),
                    SecretKey = Convert.ToBase64String(keyPair.SecretKey)
                })
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