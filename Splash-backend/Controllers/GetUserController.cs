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
            SqlCommand command = new SqlCommand("SELECT users.username FROM users WHERE uid = " + uid, con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                Dictionary<string, object>  response = new Dictionary<string, object>();
                response.Add("uid", uid);
                response.Add("user", reader[0]);
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

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
