using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Text;
using AngelesTandas.Application;

namespace AngelesTandas.Infrastructure.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly IConfiguration _config;
        private readonly byte[] _aesKey;

        public SecurityService(IConfiguration config)
        {
            _config = config;
            var keyBase64 = _config["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key is missing in configuration");
            _aesKey = Convert.FromBase64String(keyBase64);
            if (_aesKey.Length != 32) throw new InvalidOperationException("Encryption key must be 32 bytes (AES-256)");
        }

        // PBKDF2
        public string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            var result = new byte[1 + salt.Length + hash.Length];
            result[0] = 0; // version
            Buffer.BlockCopy(salt, 0, result, 1, salt.Length);
            Buffer.BlockCopy(hash, 0, result, 1 + salt.Length, hash.Length);

            return Convert.ToBase64String(result);
        }

        public bool VerifyPassword(string hash, string password)
        {
            var bytes = Convert.FromBase64String(hash);
            if (bytes[0] != 0) return false;
            var salt = new byte[16];
            Buffer.BlockCopy(bytes, 1, salt, 0, salt.Length);
            var storedHash = new byte[32];
            Buffer.BlockCopy(bytes, 1 + salt.Length, storedHash, 0, storedHash.Length);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(storedHash, computed);
        }

        // AES-256-CBC encrypt/decrypt
        public string Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plain);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipher)
        {
            var bytes = Convert.FromBase64String(cipher);
            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[aes.BlockSize / 8];
            Buffer.BlockCopy(bytes, 0, iv, 0, iv.Length);
            var cipherBytes = new byte[bytes.Length - iv.Length];
            Buffer.BlockCopy(bytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}