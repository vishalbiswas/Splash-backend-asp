using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Splash_backend.Models;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("promote")]
    public class PromotionController : Controller
    {
        [HttpPost("{uid}")]
        public Dictionary<string, object> Post(long uid, [FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (!(Program.users.TryGetValue(sessionid, out User user) && user.mod > 0))
            {
                response.Add("status", 1);
                response.Add("msg", "Invalid session");
                return response;
            }
            if (user.banned)
            {
                response.Add("status", 4);
                response.Add("msg", "You are banned from doing this");
                return response;
            }
            if (!UserController.CanMod(user.mod, uid))
            {
                response.Add("status", 3);
                response.Add("msg", "User is in a higher tier or does not exist");
                return response;
            }

            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("UPDATE users SET ismod = 1 WHERE uid = @uid", con);
            command.Parameters.AddWithValue("uid", uid);

            con.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                response.Add("status", 0);
                response.Add("msg", "User promoted to moderator");
            }
            else
            {
                response.Add("status", 5);
                response.Add("msg", "Internal error occured");
            }
            con.Close();
            return response;
        }
    }

    [Produces("application/json")]
    [Route("demote")]
    public class DemotionController : Controller
    {
        [HttpPost("{uid}")]
        public Dictionary<string, object> Post(long uid, [FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (!(Program.users.TryGetValue(sessionid, out User user) && user.mod > 0))
            {
                response.Add("status", 1);
                response.Add("msg", "Invalid session");
                return response;
            }
            if (user.banned)
            {
                response.Add("status", 4);
                response.Add("msg", "You are banned from doing this");
                return response;
            }
            if (!UserController.CanMod(user.mod, uid))
            {
                response.Add("status", 3);
                response.Add("msg", "User is in a higher tier or does not exist");
                return response;
            }

            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("UPDATE users SET ismod = 0 WHERE uid = @uid", con);
            command.Parameters.AddWithValue("uid", uid);

            con.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                response.Add("status", 0);
                response.Add("msg", "User demoted from moderator");
            }
            else
            {
                response.Add("status", 5);
                response.Add("msg", "Internal error occured");
            }
            con.Close();
            return response;
        }
    }
}