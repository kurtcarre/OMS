namespace OMS.AuthZ.Models
{
    public enum Permission : int
    {
        None = 0,
        Read = 1,
        Write = 2,
        Create = 3,
        Full = 4
    }
}