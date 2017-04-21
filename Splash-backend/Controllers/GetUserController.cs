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
    public class GetUserController : Controller
    {
        // GET api/values
        [HttpGet("{uid}")]
        public IActionResult Get(long uid)
        {
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT users.username, users.email, users.fname, users.lname, users.profpic, users.picsize FROM users WHERE uid = " + uid, con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                Dictionary<string, object>  response = new Dictionary<string, object>();
                response.Add("status", 0);
                response.Add("user", reader.GetString(0));

                if (!string.IsNullOrEmpty(reader.GetString(2))) response.Add("fname", reader[2]);
                if (!string.IsNullOrEmpty(reader.GetString(3))) response.Add("lname", reader[3]);

                response.Add("uid", uid);
                response.Add("email", reader.GetString(1));
                if (!reader.IsDBNull(4))
                {
                    byte[] rawData = new byte[reader.GetInt64(5)];
                    reader.GetBytes(0, 0, rawData, 0, rawData.Length);
                    response.Add("profpic", System.Convert.ToBase64String(rawData));
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
