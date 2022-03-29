using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Statistic.Helpers
{
    public static class AuthenticationHelper
    {
        public static string GetClaim(this ClaimsPrincipal user, string claimName)
        {
            return user.Claims.FirstOrDefault(x => x.Type == claimName)?.Value;
        }
        public static int GetId(this ClaimsPrincipal user)
        {
            var value = user.GetClaim(ClaimTypes.Name);
            if (string.IsNullOrEmpty(value))
                return 0;
            return int.Parse(value);
        }
    }
}
