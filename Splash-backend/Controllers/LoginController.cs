using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Splash_backend.Models;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("login")]
    public class LoginController : Controller
    {
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
                    if (etime.CompareTo(DateTime.Now) < 0)
                    {
                        response.Add("status", 3);
                        response.Add("msg", "Session expired. Please, re-login.");
                        return new ObjectResult(response);
                    }
                    command.CommandText = "SELECT users.uid, users.email, users.fname, users.lname, users.profpic, users.ismod, users.username, users.banned, users.canpost, users.cancomment FROM users WHERE users.uid = " + (long)reader["uid"];
                }
                reader.Dispose();
            }
            else
            {
                command.CommandText = "SELECT users.uid, users.email, users.fname, users.lname, users.profpic, users.ismod, users.username, users.banned, users.canpost, users.cancomment FROM users WHERE users.username = '" + user + "' AND users.password = '" + pass + "';";
            }
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                if (reader.GetBoolean(7))
                {
                    response.Add("status", 4);
                    response.Add("msg", "You have been banned");
                    return new ObjectResult(response);
                }
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
                response.Add("canpost", reader.GetBoolean(8));
                response.Add("cancomment", reader.GetBoolean(9));

                User loginUser = new User() { uid = reader.GetInt64(0), mod = reader.GetInt32(5), banned = reader.GetBoolean(7), canpost = reader.GetBoolean(8), cancomment = reader.GetBoolean(9) };

                if (sessionlogin)
                {
                    AddUserToSessions(sessionid, loginUser);
                    response.Add("sessionid", sessionid);
                    command.CommandText = "UPDATE sessions SET etime='" + DateTime.Now.AddDays(7) + "' WHERE sessionid='" + HttpContext.Session.Id + "';";
                }
                else
                {
                    AddUserToSessions(HttpContext.Session.Id, loginUser);
                    response.Add("sessionid", HttpContext.Session.Id);
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

        private static void AddUserToSessions(string sessionid, User user)
        {
            RemoveSession(sessionid);
            Program.users.Add(sessionid, user);
            if (Program.sessions.TryGetValue(user.uid, out List<string> sessions))
            {
                if (sessions.Count >= 5)
                {
                    string oldsession = sessions[0];
                    sessions.RemoveAt(0);
                    Program.users.Remove(oldsession);
                }
                sessions.Add(sessionid);
            }
            else
            {
                sessions = new List<string>();
                sessions.Add(sessionid);
                Program.sessions.Add(user.uid, sessions);
            }
        }

        internal static void RemoveSession(string sessionid)
        {
            if (Program.users.TryGetValue(sessionid, out User user))
            {
                Program.sessions[user.uid].Remove(sessionid);
                Program.users.Remove(sessionid);
            }
        }
    }

    [Produces("application/json")]
    [Route("logout")]
    public class LogoutController : Controller
    {
        [HttpPost]
        public Dictionary<string, object> Post([FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("UPDATE sessions SET etime='" + DateTime.Now + "' WHERE sessionid='" + sessionid + "';", con);
            Program.users.Remove(sessionid);
            con.Open();
            if (command.ExecuteNonQuery() == 1)
            {
                response.Add("status", 0);
            }
            else
            {
                response.Add("status", 1);
            }
            con.Close();
            return response;
        }
    }
}
