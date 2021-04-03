using System;
using System.Security.Claims;

namespace OMS.Auth
{
    public class UserClaim<TKey> where TKey : IEquatable<TKey>
    {
        public virtual int Id { get; set; }

        public virtual TKey UserId { get; set; }

        public virtual string ClaimType { get; set; }

        public virtual string ClaimValue { get; set; }

        public virtual Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }

        public virtual void InitialiseFromClaim(Claim claim)
        {
            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }
    }
}