using System.Collections.Generic;

namespace OMS.Auth
{
    public interface ILookupProtectorKeyRing
    {
        string CurrentKeyId { get; }
        string this[string keyId] { get; }
        IEnumerable<string> GetAllKeyIds();
    }
}