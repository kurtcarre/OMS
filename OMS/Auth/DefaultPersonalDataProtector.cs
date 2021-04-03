using System;

namespace OMS.Auth
{
    public class DefaultPersonalDataProtector : IPersonalDataProtector
    {
        private readonly ILookupProtectorKeyRing _keyRing;
        private readonly ILookupProtector _encryptor;

        public DefaultPersonalDataProtector(ILookupProtectorKeyRing keyRing, ILookupProtector protector)
        {
            _keyRing = keyRing;
            _encryptor = protector;
        }

        public virtual string UnProtect(string data)
        {
            int split = data.IndexOf(':');
            if (split == 1 || split == data.Length - 1)
                throw new InvalidOperationException("Malformed data!");

            string keyId = data.Substring(0, split);
            return _encryptor.UnProtect(keyId, data.Substring(split + 1));
        }

        public virtual string Protect(string data)
        {
            string current = _keyRing.CurrentKeyId;
            return current + ":" + _encryptor.Protect(current, data);
        }
    }
}