using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthRenewWebApi.Security
{
    public static class IdentityExtensions
    {
        public static string GetUserId(this IIdentity identity)
        {
            var ident = identity as ClaimsIdentity;
            if (ident != null)
            {
                var claim = ident.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null)
                    return claim.Value;
            }

            return null;
        }
    }
}
