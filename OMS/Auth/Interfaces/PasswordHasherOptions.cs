using System.Security.Cryptography;

namespace OMS.Auth
{
    public class PasswordHasherOptions
    {
        private static readonly RandomNumberGenerator _defaultRng = RandomNumberGenerator.Create();

        public int IterationCount { get; set; } = 10000;

        internal RandomNumberGenerator Rng { get; set; } = _defaultRng;
    }
}