using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class LoginController : Controller
    {
        [HttpGet]
        public ObjectResult Get()
        {
            Dictionary<string, object> response = new Dictionary<string, object>
            {
                { "status", 3 },
                { "msg", "Request type not supported" }
            };
            return new ObjectResult(response);
        }

        [HttpPost]
        public ObjectResult Post([FromForm]string user, [FromForm]string pass, [FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            bool sessionlogin = false;
            if ((user != null && pass != null) || sessionid != null)
            {
                if (sessionid == null)
                {
                    user = user.Trim();
                    pass = pass.Trim();
                    if (user.Length == 0 || pass.Length == 0)
                    {
                        response.Add("status", 2);
                        response.Add("msg", "Incomplete data received");
                        return new ObjectResult(response);
                    }
                }
                else sessionlogin = true;
            }
            else
            {
                response.Add("status", 2);
                response.Add("msg", "Incomplete data received");
                return new ObjectResult(response);
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand();
            SqlDataReader reader;
            command.Connection = con;
            if (sessionlogin)
            {
                command.CommandText = "SELECT * FROM sessions WHERE sessionid='" + sessionid + "';";
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    DateTime etime = (DateTime)reader["etime"];
                    if (etime.CompareTo(new DateTime()) < 0)
                    {
                        response.Add("status", 4);
                        response.Add("msg", "Session expired. Please, re-login.");
                        return new ObjectResult(response);
                    }
                    command.CommandText = "SELECT users.uid, users.email, users.fname, users.lname, users.profpic, users.ismod, users.username FROM users WHERE users.uid = " + (long)reader["uid"];
                }
                reader.Dispose();
            }
            else
            {
                command.CommandText = "SELECT users.uid, users.email, users.fname, users.lname, users.profpic, users.ismod, users.username FROM users WHERE users.username = '" + user + "' AND users.password = '" + pass + "';";
            }
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                response.Add("status", 0);
                response.Add("msg", "Login success");
                response.Add("user", reader["username"]);

                if (!reader.IsDBNull(2)) response.Add("fname", reader[2]);
                if (!reader.IsDBNull(3)) response.Add("lname", reader[3]);

                response.Add("uid", reader.GetInt64(0));
                response.Add("email", reader.GetString(1));
                if (!reader.IsDBNull(4))
                {
                    response.Add("profpic", reader.GetInt64(4));
                }
                response.Add("mod", reader.GetInt32(5));
                Program.users.Add(HttpContext.Session.Id, new User() { uid = reader.GetInt64(0), mod = reader.GetInt32(5) });
                response.Add("sessionid", HttpContext.Session.Id);
                if (sessionlogin)
                {
                    command.CommandText = "UPDATE sessions SET etime='" + new DateTime().AddDays(7) + "' WHERE sessionid='" + HttpContext.Session.Id + "';";
                }
                else
                {
                    command.CommandText = "INSERT INTO sessions VALUES ( @sessionid, @uid, @etime );";
                    command.Parameters.AddWithValue("sessionid", HttpContext.Session.Id);
                    command.Parameters.AddWithValue("uid", reader.GetInt64(0));
                    command.Parameters.AddWithValue("etime", DateTime.Now.AddDays(7));
                }
                reader.Dispose();
                command.ExecuteNonQuery();
                con.Close();
                return new ObjectResult(response);
            }
            else
            {
                response.Add("status", 1);
                response.Add("msg", "Invalid credentials");
                reader.Dispose();
                con.Close();
                return new ObjectResult(response);
            }
        }
    }
}
