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
    [Route("upload")]
    public class UploadController : Controller
    {
        [HttpPost]
        public ObjectResult Post([FromForm]string attach)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            byte[] image = Convert.FromBase64String(attach);
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("INSERT INTO attachments (image, size) OUTPUT INSERTED.attachid VALUES(@image, @size);", con);
            command.Parameters.AddWithValue("image", image);
            command.Parameters.AddWithValue("size", image.Length);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                response.Add("status", 0);
                response.Add("attachid", (long)reader["attachid"]);
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }

    }
}
