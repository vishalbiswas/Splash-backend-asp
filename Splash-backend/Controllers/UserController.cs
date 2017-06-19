using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        [HttpGet("{uid}")]
        public IActionResult Get(long uid)
        {
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT users.username, users.email, users.fname, users.lname, users.profpic, users.ismod, users.banned, users.canpost, users.cancomment FROM users WHERE uid = " + uid, con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                Dictionary<string, object>  response = new Dictionary<string, object>();
                response.Add("status", 0);
                response.Add("username", reader.GetString(0));

                if (!reader.IsDBNull(2)) response.Add("fname", reader[2]);
                if (!reader.IsDBNull(3)) response.Add("lname", reader[3]);

                response.Add("uid", uid);
                response.Add("email", reader.GetString(1));
                if (!reader.IsDBNull(4))
                {
                    response.Add("profpic", reader.GetInt64(4));
                }
                response.Add("mod", reader.GetInt32(5));
                response.Add("banned", reader["banned"]);
                response.Add("canpost", reader["canpost"]);
                response.Add("cancomment", reader["cancomment"]);
                reader.Dispose();
                con.Close();
                return new ObjectResult(response);
            }
            else
            {
                reader.Dispose();
                con.Close();
                return NotFound();
            }
        }

        internal static bool CanMod(int moderator, long toBeModerated)
        {
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand nestedCommand = new SqlCommand("SELECT ismod FROM users WHERE users.uid = " + toBeModerated, con);
            con.Open();
            SqlDataReader reader = nestedCommand.ExecuteReader();
            if (reader.Read())
            {
                return (reader.GetInt32(0) <= moderator);
            }
            con.Close();
            return false;
        }
    }
}
