using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("post")]
    public class PostController : Controller
    {
        [HttpPost]
        public ObjectResult Post([FromForm]string title, [FromForm]string content, [FromForm]long author, [FromForm]int topicid, [FromForm]long attachid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("INSERT INTO threads (title, content, creator_id, topicid, attachid) OUTPUT INSERTED.threadid, INSERTED.ctime, INSERTED.mtime VALUES (@title, @content, @creator_id, @topicid, @attachid);", con);
            command.Parameters.AddWithValue("title", title);
            command.Parameters.AddWithValue("content", content);
            command.Parameters.AddWithValue("creator_id", author);
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
                response.Add("ctime", Program.toUnixTimestamp(reader.GetDateTime(1)));
                response.Add("mtime", Program.toUnixTimestamp(reader.GetDateTime(2)));
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }
}
