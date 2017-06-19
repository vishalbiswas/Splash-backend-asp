using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("threads")]
    public class ThreadsController : Controller
    {
        [HttpGet("{quantity}")]
        public IActionResult Get(int quantity)
        {
            return Post(quantity, null, null);
        }

        [HttpPost("{quantity}")]
        public IActionResult Post(int quantity, [FromForm]string after, [FromForm]string sessionid)
        {
            string cmdText;
            if (sessionid == null)
            {
                cmdText = "select top " + quantity + ThreadController.columns + "from threads left join attachments on threads.attachid=attachments.attachid where hidden=0 ";
                if (after != null)
                {
                    cmdText += "and mtime < @after ";
                }
            }
            else
            {
                if (Program.users.TryGetValue(sessionid, out User user) && user.mod > 0) {
                    cmdText = "select" + ThreadController.columns + "from threads left join attachments on threads.attachid=attachments.attachid ";
                    if (after != null)
                    {
                        cmdText += "where mtime < @after ";
                    }
                    if (user.banned)
                    {
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error.Add("status", 4);
                        error.Add("msg", "You are banned from doing this");
                        return new ObjectResult(error);
                    }
                } else {
                    return StatusCode(401);
                }
            }
            cmdText += "order by threads.mtime desc;";
            List<Thread> response = new List<Thread>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand(cmdText, con);
            if (after != null)
            {
                command.Parameters.AddWithValue("after", Program.FromJavaTimestamp(Convert.ToInt64(after)));
            }
            response = ThreadController.GetThreadsFromReader(command.ExecuteReader(), sessionid, quantity);
            con.Close();
            return new ObjectResult(response);
        }
    }

    [Produces("application/json")]
    [Route("thread")]
    public class ThreadController : Controller
    {
        public const string columns = " threads.*, attachments.type, attachments.filename ";

        [HttpPost("{threadid}")]
        public IActionResult Post(long threadid, [FromForm]string sessionid)
        {
            string cmdText = "select" + columns + "from threads left join attachments on threads.attachid=attachments.attachid where threadid = " + threadid;
            if (sessionid == null || !(Program.users.TryGetValue(sessionid, out User user) && user.mod > 0))
            {
                cmdText += " and hidden=0 ";
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand(cmdText, con);
            SqlDataReader reader = command.ExecuteReader();
            Thread thread;
            if (reader.Read())
            {
                thread = GetSingleThreadFromReader(reader, sessionid);
            }
            else
            {
                return NotFound();
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(thread);
        }

        internal static bool IsLocked(long threadid)
        {
            bool result = true;
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT threads.locked, threads.topicid FROM threads WHERE threadid = " + threadid, con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                result = reader.GetBoolean(0);
            }
            if (!result)
            {
                result = TopicController.IsLocked(reader.GetInt32(1));
            }
            reader.Dispose();
            con.Close();
            return result;
        }

        internal static List<Thread> GetThreadsFromReader(SqlDataReader reader, string sessionid, int quantity)
        {
            List<Thread> response = new List<Thread>();
            while (reader.Read())
            {
                Thread thread = GetSingleThreadFromReader(reader, sessionid);
                if (sessionid != null)
                {
                    if (thread.needmod)
                    {
                        response.Add(thread);
                    }
                }
                else
                {
                    response.Add(thread);
                }
                if (sessionid != null && response.Count == quantity) break;
            }
            reader.Dispose();
            return response;
        }

        internal static Thread GetSingleThreadFromReader(SqlDataReader reader, string sessionid)
        {
            Thread thread = new Thread();
            thread.threadid = (long)reader["threadid"];
            thread.title = (string)reader["title"];
            thread.content = (string)reader["content"];
            thread.author = (long)reader["creator_id"];
            thread.ctime = Program.ToUnixTimestamp((DateTime)reader["ctime"]);
            thread.mtime = Program.ToUnixTimestamp((DateTime)reader["mtime"]);
            thread.topicid = (int)reader["topicid"];
            thread.hidden = (bool)reader["hidden"];
            thread.locked = (bool)reader["locked"];
            thread.reported = (int)reader["reported"];
            if (!reader.IsDBNull(reader.GetOrdinal("attachid")))
            {
                thread.attachid = (long)reader["attachid"];
                thread.type = (string)reader["type"];
                if (!reader.IsDBNull(reader.GetOrdinal("filename")))
                {
                    thread.filename = (string)reader["filename"];
                }
                else
                {
                    thread.filename = thread.threadid.ToString();
                }
            }
            else
            {
                thread.attachid = -1;
                thread.type = null;
            }
            if (thread.reported > 0)
            {
                thread.needmod = true;
            }
            else if (sessionid != null)
            {
                // only do heavy work if mod is requested
                SqlCommand nestedCommand = new SqlCommand("SELECT reported FROM comments WHERE threadid=" + thread.threadid, new SqlConnection(Program.Configuration["connectionStrings:splashConString"]));
                nestedCommand.Connection.Open();
                SqlDataReader nestedReader = nestedCommand.ExecuteReader();
                while (nestedReader.Read())
                {
                    if (nestedReader.GetInt32(0) > 0)
                    {
                        thread.needmod = true;
                        break;
                    }
                }
                nestedReader.Dispose();
                nestedCommand.Connection.Close();
            }
            return thread;
        }
    }

    [Produces("application/json")]
    [Route("search")]
    public class SearchController : Controller
    {
        [HttpGet("{query}")]
        public List<Thread> Post(string query, string sessionid, int quantity)
        {
            List<Thread> response = new List<Thread>();
            if (query == null)
            {
                return response;
            }
            query = query.Trim();
            query = String.Format("%{0}%", query);
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            string cmdText = "select top " + quantity + ThreadController.columns + "from threads left join attachments on threads.attachid=attachments.attachid where threads.title like @query or threads.content like @query ";
            if (sessionid == null || !(Program.users.TryGetValue(sessionid, out User user) && user.mod > 0))
            {
                cmdText += " and hidden=0 ";
            }
            cmdText += "order by threads.mtime desc;";
            SqlCommand command = new SqlCommand(cmdText, con);
            command.Parameters.AddWithValue("query", query);
            SqlDataReader reader = command.ExecuteReader();
            response = ThreadController.GetThreadsFromReader(reader, sessionid, -1);
            reader.Dispose();
            con.Close();
            return response;
        }
    }
}
