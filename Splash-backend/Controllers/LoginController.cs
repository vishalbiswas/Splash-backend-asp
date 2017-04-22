using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class LoginController : Controller
    {
        [HttpGet]
        public ObjectResult Get()
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            response.Add("status", 3);
            response.Add("msg", "Request type not supported");
            return new ObjectResult(response);
        }
        
        [HttpPost]
        public ObjectResult Post([FromForm]string user, [FromForm]string pass)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (user != null && pass != null) {
            user = user.Trim();
            pass = pass.Trim();
                if (user.Length == 0 || pass.Length == 0)
                {
                    response.Add("status", 2);
                    response.Add("msg", "Incomplete data received");
                    return new ObjectResult(response);
                }
            } else {
                response.Add("status", 2);
                response.Add("msg", "Incomplete data received");
                return new ObjectResult(response);
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT users.uid, users.email, users.fname, users.lname, users.profpic FROM users WHERE users.username = '" + user + "' AND users.password = '" + pass + "';", con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                response.Add("status", 0);
                response.Add("msg", "Login success");
                response.Add("user", user);

                if (!reader.IsDBNull(2)) response.Add("fname", reader[2]);
                if (!reader.IsDBNull(3)) response.Add("lname", reader[3]);

                response.Add("uid", reader.GetInt64(0));
                response.Add("email", reader.GetString(1));
                if (!reader.IsDBNull(4)) {
                    response.Add("profpic", reader.GetInt64(4));
                }
                reader.Dispose();
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
