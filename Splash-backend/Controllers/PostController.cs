using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
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
            if (!user.canpost || user.banned)
            {
                response.Add("status", 4);
                response.Add("msg", "You are not allowed to create threads");
                return response;
            }
            if (TopicController.IsLocked(topicid))
            {
                response.Add("status", 5);
                response.Add("msg", "This topic is locked from further modifications");
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

    [Produces("application/json")]
    [Route("editpost")]
    public class EditPostController : Controller
    {
        [HttpPost("{threadid}")]
        public Dictionary<string, object> Post(int threadid, [FromForm]string title, [FromForm]string content, [FromForm]int topicid, [FromForm]long attachid, [FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (!Program.users.TryGetValue(sessionid, out User user))
            {
                response.Add("status", 1);
                response.Add("msg", "Invalid session");
                return response;
            }
            if (!user.canpost || user.banned)
            {
                response.Add("status", 4);
                response.Add("msg", "You are not allowed to edit threads");
                return response;
            }
            if (ThreadController.IsLocked(threadid))
            {
                response.Add("status", 5);
                response.Add("msg", "This thread is locked from further modifications");
                return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("UPDATE threads SET threads.title=@title, threads.content=@content, threads.topicid=@topicid, threads.attachid=@attachid , threads.mtime=@mtime WHERE threads.threadid=@threadid and threads.creator_id = @uid;", con);
            command.Parameters.AddWithValue("title", title);
            command.Parameters.AddWithValue("content", content);
            command.Parameters.AddWithValue("topicid", topicid);
            command.Parameters.AddWithValue("uid", user.uid);
            if (attachid == 0)
            {
                command.Parameters.AddWithValue("attachid", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("attachid", attachid);
            }
            // Do this in SQL DB to prevent time difference if located on separate systems
            DateTime mtime = DateTime.UtcNow;
            command.Parameters.AddWithValue("mtime", mtime);
            command.Parameters.AddWithValue("threadid", threadid);
            if (command.ExecuteNonQuery() > 0)
            {
                response.Add("status", 0);
                response.Add("mtime", mtime);
            }
            con.Close();
            return response;
        }
    }
}
