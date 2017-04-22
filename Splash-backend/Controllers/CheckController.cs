using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("check")]
    public class CheckController : Controller
    {
        [HttpGet("{data}")]
        public ObjectResult Get(string data)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            response.Add("available", check(data));
            return new ObjectResult(response);
        }

        public bool check(string data)
        {
            bool result;

            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand();
            if (data.Contains("@"))
            {
                command.CommandText = "SELECT users.uid FROM users WHERE users.email = @data";
            }
            else
            {
                command.CommandText = "SELECT users.uid FROM users WHERE users.username = @data";
            }
            command.Parameters.AddWithValue("data", data);
            command.Connection = con;
            con.Open();
            SqlDataReader reader = command.ExecuteReader();
            result = !reader.HasRows;
            reader.Dispose();
            con.Close();

            return result;
        }
    }
}
