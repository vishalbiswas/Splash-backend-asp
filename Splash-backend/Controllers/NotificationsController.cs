using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("notifications")]
    public class NotificationsController : Controller
    {
        [HttpPost("{op}")]
        public IActionResult Post(string op, [FromForm]string sessionid, [FromForm]long notifyid)
        {
            if (op == "all")
            {
                return GetNotifications(sessionid, false);
            }
            else if (op == "unread")
            {
                return GetNotifications(sessionid, true);
            }
            else if (op == "done")
            {
                Dictionary<string, object> response = new Dictionary<string, object>();
                SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
                con.Open();
                SqlCommand command = new SqlCommand("UPDATE notifications SET done = 1 WHERE id = " + notifyid, con);
                if (command.ExecuteNonQuery() != 1)
                {
                    response.Add("status", 0);
                }
                con.Close();
                return new ObjectResult(response);
            }
            else return NotFound();
        }
        
        private IActionResult GetNotifications(string sessionid, bool unread)
        {
            List<Notification> response = new List<Notification>();
            if (!Program.users.TryGetValue(sessionid, out User user))
            {
                return null;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT * from notifications WHERE uid = " + user.uid, con);
            if (unread)
            {
                command.CommandText += " AND done = 0;";
            }
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Notification notification = new Notification();
                notification.notifyid = (long)reader["id"];
                notification.uid = (long)reader["uid"];
                notification.code = (int)reader["code"];
                notification.done = (bool)reader["done"];
                if (reader.IsDBNull(reader.GetOrdinal("custom"))) notification.custom = "";
                else notification.custom = (string)reader["custom"];
                if (reader.IsDBNull(reader.GetOrdinal("commentid"))) notification.commentid = -1;
                else notification.commentid = (long)reader["commentid"];
                if (reader.IsDBNull(reader.GetOrdinal("threadid"))) notification.threadid = -1;
                else notification.threadid = (long)reader["threadid"];
                if (reader.IsDBNull(reader.GetOrdinal("actionuid"))) notification.actionuid = -1;
                else notification.actionuid = (long)reader["actionuid"];
                response.Add(notification);
            }
            con.Close();
            return new ObjectResult(response);
        }
    }
}
