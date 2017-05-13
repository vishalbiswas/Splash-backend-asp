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
            List<Thread> response = new List<Thread>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("select top " + quantity + " * from threads order by threads.mtime desc;", con);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Thread thread = new Thread();
                thread.threadid = (long)reader["threadid"];
                thread.title = (string)reader["title"];
                thread.content = (string)reader["content"];
                thread.author = (long)reader["creator_id"];
                thread.ctime = Program.toUnixTimestamp((DateTime)reader["ctime"]);
                thread.mtime = Program.toUnixTimestamp((DateTime)reader["mtime"]);
                thread.topicid = (int)reader["topicid"];
                if (!reader.IsDBNull(reader.GetOrdinal("attachid")))
                {
                    thread.attachid = (long)reader["attachid"];
                }
                else
                {
                    thread.attachid = -1;
                }
                response.Add(thread);
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }
}
