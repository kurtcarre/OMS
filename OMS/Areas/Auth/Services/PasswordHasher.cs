using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

namespace OMS.Auth.Services
{
    public class PasswordHasher
    {
        private readonly int _iterCount;
        private readonly RandomNumberGenerator rng;

        public PasswordHasher()
        {
            _iterCount = 1000;
            if (_iterCount < 1)
                throw new InvalidOperationException("Iteration count must be a positive integer!");

            rng = RandomNumberGenerator.Create();
        }

        public string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            byte[] hash = HashPassword(password, prf: KeyDerivationPrf.HMACSHA512, iterCount: _iterCount, saltSize: 128/8, numBytesRequested: 256/8);

            return Convert.ToBase64String(hash);
        }

        private byte[] HashPassword(string password, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
        {
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);
            byte[] subKey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subKey.Length];
            outputBytes[0] = 0x01;
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subKey, 0, outputBytes, 13 + saltSize, subKey.Length);
            return outputBytes;
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string suppliedPassword)
        {
            if (hashedPassword == null)
                throw new ArgumentNullException(nameof(hashedPassword));
            if (suppliedPassword == null)
                throw new ArgumentNullException(nameof(suppliedPassword));

            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            if (decodedHashedPassword.Length == 0)
                return PasswordVerificationResult.Failed;

            int embeddedIterCount;
            if(VerifyHashedPassword(decodedHashedPassword, suppliedPassword, out embeddedIterCount))
            {
                return (embeddedIterCount < _iterCount)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Success;
            }

            return PasswordVerificationResult.Failed;
        }

        private bool VerifyHashedPassword(byte[] hashedPassword, string password, out int iterCount)
        {
            iterCount = default(int);

            try
            {
                KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

                if (saltLength < 128 / 8)
                    return false;

                byte[] salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, saltLength);

                int subKeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subKeyLength < 128 / 8)
                    return false;

                byte[] expectedSubKey = new byte[subKeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubKey, 0, expectedSubKey.Length);

                byte[] actualSubKey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, expectedSubKey.Length);

                return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
            }

            catch
            {
                return false;
            }
        }

        private uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24) | ((uint)(buffer[offset + 1]) << 16 | ((uint)(buffer[offset + 2]) << 8) | (uint)(buffer[offset + 3]));
        }

        private void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }
    }
}