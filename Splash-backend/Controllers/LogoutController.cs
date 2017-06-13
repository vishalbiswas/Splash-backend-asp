using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("logout")]
    public class LogoutController : Controller
    {
        [HttpPost]
        public Dictionary<string, object> Post([FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            Program.users.Remove(sessionid);
            response.Add("status", 0);
            return response;
        }
    }
}
