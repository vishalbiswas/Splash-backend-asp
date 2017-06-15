using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("register")]
    public class RegisterController : Controller
    {
        [HttpPost]
        public ObjectResult Post([FromForm]string user, [FromForm]string email, [FromForm]string pass)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (user == null || email == null || pass == null)
            {
                response.Add("status", -1);
                return new ObjectResult(response);
            }

            user = user.Trim();
            email = email.Trim();
            pass = pass.Trim();

            CheckController checker = new CheckController();
            if (user.Length == 0 || !checker.Check(user))
            {
                response.Add("status", 2);
                return new ObjectResult(response);
            }
            if (email.Length == 0 || !(new EmailAddressAttribute().IsValid(email)) || !checker.Check(email))
            {
                response.Add("status", 3);
                return new ObjectResult(response);
            }
            if (pass.Length < 8)
            {
                response.Add("status", 4);
                return new ObjectResult(response);
            }

            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("INSERT INTO users (username, email, password) VALUES (@username, @email, @password);", con);
            command.Parameters.AddWithValue("username", user);
            command.Parameters.AddWithValue("email", email);
            command.Parameters.AddWithValue("password", pass);

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

            return new ObjectResult(response);
        }
    }

    [Produces("application/json")]
    [Route("check")]
    public class CheckController : Controller
    {
        [HttpGet("{data}")]
        public ObjectResult Get(string data)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            response.Add("available", Check(data));
            return new ObjectResult(response);
        }

        public bool Check(string data)
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
