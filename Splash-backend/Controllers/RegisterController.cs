using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            if (user.Length == 0 || !checker.check(user))
            {
                response.Add("status", 2);
                return new ObjectResult(response);
            }
            if (email.Length == 0 || !(new EmailAddressAttribute().IsValid(email)) || !checker.check(email))
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
}
