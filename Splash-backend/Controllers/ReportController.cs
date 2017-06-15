using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("report")]
    public class ReportController : Controller
    {
        [HttpPost("{type}")]
        public Dictionary<string, object> Post(string type, [FromForm]long id, [FromForm]string msg, [FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (!Program.users.TryGetValue(sessionid, out User user))
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
                    cmdText = "INSERT INTO reports_threads (threadid, userid) values ( " + id + ", " + user.uid + ");";
                    break;
                case "comment":
                    cmdText = "INSERT INTO reports_comments (commentid, userid) values ( " + id + ", " + user.uid + ");";
                    break;
                case "user":
                    cmdText = "INSERT INTO reports_users (uid, userid) values ( " + id + ", " + user.uid + ");";
                    break;
                default:
                    response.Add("status", 2);
                    response.Add("msg", "Invalid report type");
                    return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand(cmdText, con);

            con.Open();
            if (command.ExecuteNonQuery() == 1)
            {
                response.Add("status", 0);
                response.Add("msg", "Reported successfully");
            }
            else
            {
                response.Add("status", 3);
                response.Add("msg", "Internal error occured while reporting");
            }
            con.Close();

            return response;
        }
    }
}
