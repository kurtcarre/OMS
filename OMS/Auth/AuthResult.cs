using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OMS.Auth
{
    public class AuthResult
    {
        private static readonly AuthResult _success = new AuthResult { Succeeded = true };
        private List<AuthError> _errors = new List<AuthError>();

        public bool Succeeded { get; protected set; }

        public IEnumerable<AuthError> Errors => _errors;

        public static AuthResult Success => _success;

        public static AuthResult Failed(params AuthError[] errors)
        {
            AuthResult result = new AuthResult { Succeeded = false };
            if(errors != null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }

        public override string ToString()
        {
            return Succeeded ?
                "Succeeded" :
                string.Format(CultureInfo.InvariantCulture, "{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x.Code).ToList()));
        }
    }
}