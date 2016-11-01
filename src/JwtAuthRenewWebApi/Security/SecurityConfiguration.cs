using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Configuration;
using JwtAuthRenewWebApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthRenewWebApi.Security
{
    public static class SecurityConfiguration
    {
        public static string SigningKey = WebConfigurationManager.AppSettings["SigningKey"];
        public static string TokenIssuer = WebConfigurationManager.AppSettings["TokenIssuer"];
        public static string TokenAudience = WebConfigurationManager.AppSettings["TokenAudience"];

        public static SecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        public static SigningCredentials SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    }
}
