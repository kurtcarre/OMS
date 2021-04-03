namespace OMS.Auth
{
    public interface ILookupProtector
    {
        string Protect(string keyId, string data);
        string UnProtect(string keyId, string data);
    }
}