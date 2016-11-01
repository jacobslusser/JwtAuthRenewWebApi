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

            // Limit our services responses to camelCase JSON only
            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));

            // Configure the authentication filter to run on every request
            config.Filters.Add(new BearerAuthenticationFilter());
        }

        // A content negotiator that serves only JSON.
        public class JsonContentNegotiator : IContentNegotiator
        {
            private readonly JsonMediaTypeFormatter formatter;

            public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            {
                var result = new ContentNegotiationResult(formatter, new MediaTypeHeaderValue("application/json"));
                return result;
            }

            public JsonContentNegotiator(JsonMediaTypeFormatter formatter)
            {
                this.formatter = formatter;
            }
        }
    }
}
