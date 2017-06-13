using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("post")]
    public class PostController : Controller
    {
        [HttpPost]
        public Dictionary<string, object> Post([FromForm]string title, [FromForm]string content, [FromForm]string sessionid, [FromForm]int topicid, [FromForm]long attachid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (!Program.users.TryGetValue(sessionid, out User user))
            {
                response.Add("status", 1);
                response.Add("msg", "Invalid session");
                return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("INSERT INTO threads (title, content, creator_id, topicid, attachid) OUTPUT INSERTED.threadid, INSERTED.ctime, INSERTED.mtime VALUES (@title, @content, @creator_id, @topicid, @attachid);", con);
            command.Parameters.AddWithValue("title", title);
            command.Parameters.AddWithValue("content", content);
            command.Parameters.AddWithValue("creator_id", user.uid);
            command.Parameters.AddWithValue("topicid", topicid);
            if (attachid == 0)
            {
                command.Parameters.AddWithValue("attachid", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("attachid", attachid);
            }
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                response.Add("status", 0);
                response.Add("threadid", reader.GetInt64(0));
                response.Add("ctime", Program.ToUnixTimestamp(reader.GetDateTime(1)));
                response.Add("mtime", Program.ToUnixTimestamp(reader.GetDateTime(2)));
            }
            reader.Dispose();
            con.Close();
            return response;
        }
    }
}
