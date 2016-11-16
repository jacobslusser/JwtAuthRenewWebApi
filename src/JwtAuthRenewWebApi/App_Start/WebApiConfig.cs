using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using JwtAuthRenewWebApi.Security;
using Newtonsoft.Json.Serialization;

namespace JwtAuthRenewWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            // Configure the authentication filter to run on every request marked with the AuthorizeAttribute
            config.Filters.Add(new BearerAuthenticationFilter());

            // Configure the sliding expiration handler to run on every request
            config.MessageHandlers.Add(new SlidingExpirationHandler());

            // Help our JSON look professional using camelCase
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
