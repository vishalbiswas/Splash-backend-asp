using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Splash_backend.Models;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("search")]
    public class SearchController : Controller
    {
        [HttpGet("{query}")]
        public List<Thread> Get(string query)
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
            SqlCommand command = new SqlCommand("select top 50 * from threads where threads.title like @query or threads.content like @query order by threads.mtime desc;", con);
            command.Parameters.AddWithValue("query", query);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Thread thread = new Thread();
                thread.threadid = (long)reader["threadid"];
                thread.title = (string)reader["title"];
                thread.content = (string)reader["content"];
                thread.author = (long)reader["creator_id"];
                thread.ctime = (DateTime)reader["ctime"];
                thread.mtime = (DateTime)reader["mtime"];
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
            return response;
        }
    }
}
