using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace JwtAuthRenewWebApi.Controllers
{
    [RoutePrefix("api/v1/ping")]
    public class PingController : ApiController
    {
        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            return Ok(new
            {
                Message = "Hello World!"
            });
        }

        [Authorize]
        [Route("authenticated")]
        public async Task<IHttpActionResult> GetAuthenticated()
        {
            // Will return 401 Unauthorized if not authenticated
            return await Get();
        }
    }
}
