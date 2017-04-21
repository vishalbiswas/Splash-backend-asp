using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IO;

namespace Splash_backend.Controllers
{
    [Produces("application/json")]
    [Route("upload")]
    public class UploadController : Controller
    {
        [HttpPost]
        public ObjectResult Post(IFormFile attach)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("INSERT INTO attachments (image, size) OUTPUT INSERTED.attachid VALUES(@image, @size);", con);
            command.Parameters.AddWithValue("image", new BinaryReader(attach.OpenReadStream()).ReadBytes(Convert.ToInt32(attach.Length)));
            command.Parameters.AddWithValue("size", attach.Length);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                response.Add("status", 0);
                response.Add("attachid", (long)reader["attachid"]);
            }
            else
            {
                response.Add("status", 1);
            }
            reader.Dispose();
            con.Close();
            Console.Write(new StreamReader(Request.Body).ReadToEnd());
            return new ObjectResult(response);
        }

    }
}
