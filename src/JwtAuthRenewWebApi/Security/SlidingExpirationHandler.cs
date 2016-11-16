using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using JwtAuthRenewWebApi.Controllers;

namespace JwtAuthRenewWebApi.Security
{
    public class SlidingExpirationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            // Preflight check 1: did the request come with a token?
            var authorization = request.Headers.Authorization;
            if (authorization == null || authorization.Scheme != "Bearer" || string.IsNullOrEmpty(authorization.Parameter))
            {
                // No token on the request
                return response;
            }

            // Preflight check 2: did that token pass authentication?
            var claimsPrincipal = request.GetRequestContext().Principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                // Not an authorized request
                return response;
            }

            // Extract the claims and put them into a new JWT
            var fullName = claimsPrincipal.Identity.Name;
            var userId = claimsPrincipal.Identity.GetUserId();
            var lifetimeInMinutes = int.Parse(WebConfigurationManager.AppSettings["TokenLifetimeInMinutes"]);

            var token = UsersController.CreateToken(userId, fullName, lifetimeInMinutes);
            response.Headers.Add("Authorization", token);

            return response;
        }
    }
}
