﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("lock")]
    public class LockController : Controller
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
                    cmdText = "UPDATE threads SET locked=1 WHERE threadid=" + id + ";";
                    break;
                case "comment":
                    cmdText = "UPDATE comments SET locked=1 WHERE commentid=" + id + ";";
                    break;
                case "user":
                    if (!UserController.CanMod(user.mod, id))
                    {
                        response.Add("status", 3);
                        response.Add("msg", "User is in a higher tier or does not exist");
                        return response;
                    }
                    cmdText = "UPDATE users SET canpost=0, cancomment=0 WHERE uid=" + id + ";";
                    break;
                default:
                    response.Add("status", 2);
                    response.Add("msg", "Invalid action object type");
                    return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand(cmdText, con);

            con.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                ReportLogger.LogAction(type, ReportLogger.LOCK, id, user.uid);
                response.Add("status", 0);
                response.Add("msg", "Object locked successfully");
            }
            else
            {
                response.Add("status", 5);
                response.Add("msg", "Internal error occured while locking object");
            }
            con.Close();

            return response;
        }
    }

    [Produces("application/json")]
    [Route("unlock")]
    public class UnlockController : Controller
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
                    cmdText = "UPDATE threads SET locked=0 WHERE threadid=" + id + ";";
                    break;
                case "comment":
                    cmdText = "UPDATE comments SET locked=0 WHERE commentid=" + id + ";";
                    break;
                case "user":
                    if (!UserController.CanMod(user.mod, id))
                    {
                        response.Add("status", 3);
                        response.Add("msg", "User is in a higher tier or does not exist");
                        return response;
                    }
                    cmdText = "UPDATE users SET canpost=1, cancomment=1 WHERE uid=" + id + ";";
                    break;
                default:
                    response.Add("status", 2);
                    response.Add("msg", "Invalid action object type");
                    return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand(cmdText, con);

            con.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                ReportLogger.LogAction(type, ReportLogger.UNLOCK, id, user.uid);
                response.Add("status", 0);
                response.Add("msg", "Object locked successfully");
            }
            else
            {
                response.Add("status", 5);
                response.Add("msg", "Internal error occured while locking object");
            }
            con.Close();

            return response;
        }
    }
}
