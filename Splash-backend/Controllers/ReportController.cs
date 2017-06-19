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


            string column;
            string table;

            switch (type)
            {
                case "thread":
                    column = "threadid";
                    table = "reports_threads";
                    break;
                case "comment":
                    column = "commentid";
                    table = "reports_comments";
                    break;
                case "user":
                    column = "uid";
                    table = "resports_users";
                    break;
                default:
                    response.Add("status", 2);
                    response.Add("msg", "Invalid report type");
                    return response;
            }

            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("SELECT reportid FROM " + table + " WHERE " + column + " = @id and userid=@uid;", con);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("uid", user.uid);
            con.Open();

            if (command.ExecuteReader().HasRows)
            {
                response.Add("status", 3);
                response.Add("msg", "You have already reported this");
                return response;
            }

            con.Close();
            con.Open();

            command.CommandText = "INSERT INTO " + table + " (" + column + ", userid) VALUES (@id, @uid)";
            if (command.ExecuteNonQuery() > 0)
            {
                response.Add("status", 0);
                response.Add("msg", "Reported successfully");
            }
            else
            {
                response.Add("status", 5);
                response.Add("msg", "Internal error occured while reporting");
            }
            con.Close();

            return response;
        }
    }

    [Produces("application/json")]
    [Route("clear")]
    public class ClearController : Controller
    {
        [HttpPost("{type}")]
        public Dictionary<string, object> Post(string type, [FromForm]long id, [FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (!Program.users.TryGetValue(sessionid, out User user) && user.mod > 0)
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
            
            string column;
            string table;

            switch (type)
            {
                case "thread":
                    column = "threadid";
                    table = "threads";
                    break;
                case "comment":
                    column = "commentid";
                    table = "comments";
                    break;
                case "user":
                    column = "uid";
                    table = "users";
                    break;
                default:
                    response.Add("status", 2);
                    response.Add("msg", "Invalid report type");
                    return response;
            }

            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("UPDATE " + table + " SET reported = 0 WHERE " + column + " = @id;", con);
            command.Parameters.AddWithValue("id", id);
            con.Open();
            
            if (command.ExecuteNonQuery() > 0)
            {
                ReportLogger.LogAction(type, ReportLogger.RELEASE, id, user.uid);
                response.Add("status", 0);
                response.Add("msg", "Reports cleared");
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
