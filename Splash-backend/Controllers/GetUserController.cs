using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        // GET api/values
        [HttpGet("{uid}")]
        public IActionResult Get(long uid)
        {
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT users.username, users.email, users.fname, users.lname, users.profpic FROM users WHERE uid = " + uid, con);
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
    }
}
