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
    [Route("threads")]
    public class ThreadsController : Controller
    {
        [HttpGet("{quantity}")]
        public ObjectResult Get(int quantity)
        {
            return Post(quantity, null);
        }

        [HttpPost("{quantity}")]
        public ObjectResult Post(int quantity, [FromForm]string sessionid)
        {
            string cmdText = "select top " + quantity + " * from threads left join attachments on threads.attachid=attachments.attachid ";
            if (sessionid == null || !(Program.users.TryGetValue(sessionid, out User user) && user.mod > 0))
            {
                cmdText += "where hidden=0 ";
            }
            cmdText += "order by threads.mtime desc;";
            List<Thread> response = new List<Thread>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand(cmdText, con);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
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
                    command.CommandText = "SELECT reported FROM comments WHERE threadid=" + thread.threadid;
                    reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        if (reader.GetInt32(0) > 0)
                        {
                            thread.needmod = true;
                            break;
                        }
                    }
                }
                response.Add(thread);
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }

    [Produces("application/json")]
    [Route("thread")]
    public class ThreadController : Controller
    {
        [HttpPost("{threadid}")]
        public IActionResult Post(long threadid, [FromForm]string sessionid)
        {
            string cmdText = "select * from threads left join attachments on threads.attachid=attachments.attachid where threadid = " + threadid;
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
                thread = new Thread();
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
                    command.CommandText = "SELECT reported FROM comments WHERE threadid=" + thread.threadid;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetInt32(0) > 0)
                        {
                            thread.needmod = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                return NotFound();
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(thread);
        }
    }
}
