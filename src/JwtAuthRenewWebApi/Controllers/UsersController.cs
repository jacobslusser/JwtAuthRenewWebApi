using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;
using JwtAuthRenewWebApi.Models;
using JwtAuthRenewWebApi.Security;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace JwtAuthRenewWebApi.Controllers
{
    [RoutePrefix("api/v1/users")]
    public class UsersController : ApiController
    {
        // Stand-in for our users table
        private static string usersJson = File.ReadAllText(HostingEnvironment.MapPath("~/App_Data/Users.json"));
        private static IList<User> users = JsonConvert.DeserializeObject<IList<User>>(usersJson);

        [Route("authenticate")]
        [HttpPost]
        public IHttpActionResult Authenticate(Credentials credentials)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            // It goes without question that in the real world our passwords would be hashed
            var user = users.FirstOrDefault(u => u.EmailAddress.Equals(credentials.EmailAddress, StringComparison.OrdinalIgnoreCase) && credentials.Password == u.Password);
            if(user == null)
                return Unauthorized();

            var lifetimeInMinutes = int.Parse(WebConfigurationManager.AppSettings["TokenLifetimeInMinutes"]);
            var token = CreateToken(user.UserId.ToString(), user.FullName, lifetimeInMinutes);

            return Ok(new
            {
                Token = token,
                LifetimeInMinutes = lifetimeInMinutes,
                FullName = user.FullName
                // Any anything else that you want here...
            });
        }

        [Authorize]
        [Route("{userId:long}")]
        public async Task<IHttpActionResult> GetUser(long userId)
        {
            // Example of using the JWT claims to ensure a user can only access their own user information
            if (userId.ToString() != User.Identity.GetUserId())
                return Unauthorized();

            // TODO
            return Ok();
        }

        public static string CreateToken(string userId, string fullName, int lifetimeInMinutes)
        {
            // Create the JWT
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("name", fullName)
                // And any other bit of (session) data you want....
            });

            var now = DateTime.UtcNow;
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = SecurityConfiguration.TokenIssuer,
                Audience = SecurityConfiguration.TokenAudience,
                SigningCredentials = SecurityConfiguration.SigningCredentials,
                IssuedAt = now,
                Expires = now.AddMinutes(lifetimeInMinutes)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
