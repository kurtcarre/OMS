using System;
using System.Collections.Generic;

namespace OMS.Auth
{
    public interface IPersonalDataProtector
    {
        string Protect(string data);
        string UnProtect(string data);
    }
}