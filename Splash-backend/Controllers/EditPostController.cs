using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("editpost")]
    public class EditPostController : Controller
    {
        [HttpPost("{threadid}")]
        public Dictionary<string, object> Post(int threadid, [FromForm]string title, [FromForm]string content, [FromForm]int topicid, [FromForm]long attachid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("UPDATE threads SET threads.title=@title, threads.content=@content, threads.topicid=@topicid, threads.attachid=@attachid , threads.mtime=@mtime WHERE threads.threadid=@threadid;", con);
            command.Parameters.AddWithValue("title", title);
            command.Parameters.AddWithValue("content", content);
            command.Parameters.AddWithValue("topicid", topicid);
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
            if (command.ExecuteNonQuery() == 1)
            {
                response.Add("status", 0);
                response.Add("mtime", mtime);
            }
            con.Close();
            return response;
        }
    }
}
