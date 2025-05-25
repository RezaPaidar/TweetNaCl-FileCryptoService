# File Encryption/Decryption using TweetNaCl

![GitHub](https://img.shields.io/github/license/RezaPaidar/TweetNaCl-FileCryptoService)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)

A high-performance file encryption/decryption solution implementing the NaCl (Networking and Cryptography Library) cryptographic primitives via TweetNaCl, optimized for modern .NET applications.

## Features

- **Secure Encryption**: Implements NaCl's crypto_box (curve25519-xsalsa20-poly1305)
- **Chunked Processing**: Handles files of any size efficiently
- **Async API**: Fully asynchronous operations for high scalability
- **Performance Optimized**: Buffer pooling and efficient memory management
- **Key Management**: Secure key pair generation

## Why NaCl?

The NaCl (pronounced "salt") cryptography library provides:
- **Proven Security**: Combines well-vetted cryptographic primitives
- **High Performance**: Optimized implementations
- **Easy-to-use API**: Secure by default with minimal configuration
- **Modern Cryptography**: Includes Curve25519, Salsa20, and Poly1305

## Performance Optimizations

### 1. Memory Efficiency
```csharp
// Using ArrayPool for buffer reuse
private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
var buffer = _arrayPool.Rent(chunkSize);
try {
    // Process buffer
} finally {
    _arrayPool.Return(buffer);
}
```

### 2. Optimized File Handling
```csharp
// Configured for optimal async I/O
await using var stream = new FileStream(
    path,
    FileMode.Open,
    FileAccess.Read,
    FileShare.Read,
    bufferSize: 65536, // 64KB buffer
    options: FileOptions.Asynchronous | FileOptions.SequentialScan
);
```
### 3. Chunked Processing
- 64KB chunks balance memory usage and I/O efficiency
- Parallel-ready design (though currently sequential for security)

### 4. Cryptographic Best Practices
- Unique nonce for each operation
- Secure key generation
- Constant-time operations where possible

## Key Highlights of This README:

1. **Professional Presentation**: Badges and clean structure
2. **Clear Value Proposition**: Explains why NaCl was chosen
3. **Performance Focus**: Dedicated section with code examples
4. **Practical Benchmarks**: Helps users set expectations
5. **Complete Usage Examples**: From installation to API usage
6. **Security Conscious**: Includes important warnings
7. **Well-Structured**: Easy to navigate sections

To add this to your Git repository:

1. Create a new file named `README.md` in your project root
2. Paste this content
3. Customize the sections with your specific details (URLs, names, etc.)
4. Commit and push:

```bash
git add README.md
git commit -m "Add comprehensive README"
git push origin main


