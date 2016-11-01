using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JwtAuthRenewWebApi.Models
{
    public class User
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }
}