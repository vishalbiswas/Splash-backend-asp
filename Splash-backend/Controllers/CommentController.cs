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
    [Route("comment")]
    public class CommentController : Controller
    {
        [HttpPost("{operation}")]
        public Dictionary<string, object> Post(string operation, [FromForm]long threadid, [FromForm]string content, [FromForm]string sessionid, [FromForm]long commentid)
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
            SqlCommand command;
            SqlDataReader reader;
            if (operation == "edit" || operation == "reply")
            {
                command = new SqlCommand("SELECT creator_id, threadid FROM comments WHERE commentid = " + commentid, con);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    if (threadid != reader.GetInt64(1))
                    {
                        response.Add("status", 3);
                        response.Add("msg", "Invalid thread");
                        reader.Dispose();
                        con.Close();
                        return response;
                    }
                    if (operation == "edit")
                    {
                        if (reader.GetInt64(0) == user.uid)
                        {
                            command = new SqlCommand(Program.COMMENT_TEMP_DDL + "UPDATE comments SET content=@content OUTPUT UPDATED.commentid, UPDATED.ctime, UPDATED.mtime, UPDATED.parent INTO @t WHERE commentid = @commentid; SELECT * FROM @t;", con);
                        }
                        else
                        {
                            response.Add("status", 1);
                            response.Add("msg", "Invalid session");
                            reader.Dispose();
                            con.Close();
                            return response;
                        }
                    }
                    else
                    {
                        command = new SqlCommand(Program.COMMENT_TEMP_DDL + "INSERT INTO comments (threadid, content, creator_id, parent) OUTPUT INSERTED.commentid, INSERTED.ctime, INSERTED.mtime, INSERTED.parent INTO @t VALUES (@threadid, @content, @creator_id, @commentid); SELECT * FROM @t;", con);
                        command.Parameters.AddWithValue("threadid", reader.GetInt64(1));
                        command.Parameters.AddWithValue("creator_id", user.uid);
                    }
                    command.Parameters.AddWithValue("commentid", commentid);
                }
                else
                {
                    response.Add("status", 2);
                    response.Add("msg", "Invalid parent comment id");
                    return response;
                }
                reader.Dispose();
            }
            else
            {
                command = new SqlCommand(Program.COMMENT_TEMP_DDL + "INSERT INTO comments (threadid, content, creator_id) OUTPUT INSERTED.commentid, INSERTED.ctime, INSERTED.mtime, INSERTED.parent INTO @t VALUES (@threadid, @content, @creator_id); SELECT * FROM @t;", con);
                command.Parameters.AddWithValue("threadid", threadid);
                command.Parameters.AddWithValue("creator_id", user.uid);
            }
            command.Parameters.AddWithValue("content", content);
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                response.Add("status", 0);
                if (operation == "reply") response.Add("parent", commentid);
                response.Add("ctime", Program.ToUnixTimestamp(reader.GetDateTime(1)));
                response.Add("mtime", Program.ToUnixTimestamp(reader.GetDateTime(2)));
                response.Add("commentid", reader.GetInt64(0));
            }
            reader.Dispose();
            con.Close();
            return response;
        }
    }
}
