using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
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
            if (!user.cancomment || user.banned)
            {
                response.Add("status", 4);
                response.Add("msg", "You are not allowed to comment");
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

    [Produces("application/json")]
    [Route("comments")]
    public class CommentsController : Controller
    {
        [HttpGet("{threadid}")]
        public ObjectResult Get(long threadid, [FromForm]string sessionid)
        {
            List<Comment> response = new List<Comment>();
            Dictionary<string, object> error = new Dictionary<string, object>();
            if (sessionid != null || !Program.users.TryGetValue(sessionid, out User user))
            {
                error.Add("status", 1);
                error.Add("msg", "Invalid session");
                return new ObjectResult(error);
            }
            if (user.banned)
            {
                error.Add("status", 4);
                error.Add("msg", "You are banned from doing this");
                return new ObjectResult(error);
            }
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
                if (!comment.hidden || user.mod > 0)
                {
                    response.Add(comment);
                }
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }
}
