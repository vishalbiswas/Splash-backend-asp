using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Splash_backend.Models;
using System.Linq;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("hide")]
    public class HideController : Controller
    {
        [HttpPost("{type}")]
        public Dictionary<string, object> Post(string type, [FromForm]long id, [FromForm]string msg, [FromForm]string sessionid)
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
            string cmdText;

            switch (type)
            {
                case "thread":
                    cmdText = "UPDATE threads SET hidden=1 WHERE threadid=" + id + ";";
                    break;
                case "comment":
                    cmdText = "UPDATE comments SET hidden=1 WHERE commentid=" + id + ";";
                    break;
                case "user":
                    cmdText = "UPDATE users SET banned=1 WHERE uid=" + id + ";";
                    if (Program.sessions.TryGetValue(id, out List<string> userSessions))
                    {
                        foreach (string session in userSessions)
                        {
                            LoginController.RemoveSession(session);
                        }
                    }
                    break;
                default:
                    response.Add("status", 2);
                    response.Add("msg", "Invalid action object type");
                    return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand(cmdText, con);

            con.Open();
            if (command.ExecuteNonQuery() == 1)
            {
                response.Add("status", 0);
                response.Add("msg", "Action successful");
            }
            else
            {
                response.Add("status", 3);
                response.Add("msg", "Internal error occured");
            }
            con.Close();

            return response;
        }
    }

    [Produces("application/json")]
    [Route("unhide")]
    public class UnhideController : Controller
    {
        [HttpPost("{type}")]
        public Dictionary<string, object> Post(string type, [FromForm]long id, [FromForm]string sessionid)
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
            string cmdText;

            switch (type)
            {
                case "thread":
                    cmdText = "UPDATE threads SET hidden=0 WHERE threadid=" + id + ";";
                    break;
                case "comment":
                    cmdText = "UPDATE comments SET hidden=0 WHERE commentid=" + id + ";";
                    break;
                case "user":
                    cmdText = "UPDATE users SET banned=0 WHERE uid=" + id + ";";
                    break;
                default:
                    response.Add("status", 2);
                    response.Add("msg", "Invalid action object type");
                    return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand(cmdText, con);

            con.Open();
            if (command.ExecuteNonQuery() == 1)
            {
                response.Add("status", 0);
                response.Add("msg", "Action successful");
            }
            else
            {
                response.Add("status", 3);
                response.Add("msg", "Internal error occured");
            }
            con.Close();

            return response;
        }
    }
}
