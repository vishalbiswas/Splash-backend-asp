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
                Comment comment = new Comment()
                {
                    commentid = (long)reader["commentid"],
                    threadid = (long)reader["threadid"],
                    content = (string)reader["content"],
                    author = (long)reader["creator_id"]
                };
                if (!reader.IsDBNull(reader.GetOrdinal("parent"))) comment.parent = (long)reader["parent"];
                else comment.parent = -1;
                comment.ctime = Program.ToUnixTimestamp((DateTime)reader["ctime"]);
                comment.mtime = Program.ToUnixTimestamp((DateTime)reader["mtime"]);
                comment.locked = (bool)reader["locked"];
                comment.hidden = (bool)reader["hidden"];
                comment.reported = (int)reader["reported"];
                response.Add(comment);
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }
}
