using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("comments")]
    public class CommentsController : Controller
    {
        [HttpGet("{threadid}")]
        public ObjectResult Get(long threadid)
        {
            List<Comment> response = new List<Comment>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("select * from comments where comments.threadid = @threadid order by comments.ctime asc;", con);
			command.Parameters.AddWithValue("threadid", threadid);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Comment comment = new Comment();
                comment.commentid = (long)reader["commentid"];
                comment.threadid = (long)reader["threadid"];
                comment.content = (string)reader["content"];
                comment.author = (long)reader["creator_id"];
                comment.ctime = Program.toUnixTimestamp((DateTime)reader["ctime"]);
                comment.mtime = Program.toUnixTimestamp((DateTime)reader["mtime"]);
                response.Add(comment);
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }
}
