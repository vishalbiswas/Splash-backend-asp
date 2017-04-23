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
    [Route("comment")]
    public class CommentController : Controller
    {
        [HttpPost("{threadid}")]
        public ObjectResult Post(long threadid, [FromForm]string content, [FromForm]long author)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("INSERT INTO comments (threadid, content, creator_id) OUTPUT INSERTED.commentid, INSERTED.ctime, INSERTED.mtime VALUES (@threadid, @content, @creator_id);", con);
			command.Parameters.AddWithValue("threadid", threadid);
            command.Parameters.AddWithValue("content", content);
            command.Parameters.AddWithValue("creator_id", author);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                response.Add("status", 0);
                response.Add("commentid", reader.GetInt64(0));
                response.Add("ctime", reader.GetDateTime(1));
                response.Add("mtime", reader.GetDateTime(2));
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }
}
