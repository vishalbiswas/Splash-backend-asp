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
    [Route("attachment")]
    public class AttachmentController : Controller
    {
        [HttpGet("{attachid}")]
        public ObjectResult Get(long attachid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT attachments.image, attachments.size FROM attachments WHERE attachments.attachid = " + attachid + ";", con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                response.Add("image", Convert.ToBase64String((byte[])reader["image"]));
            }
            reader.Dispose();
            con.Close();
            return new ObjectResult(response);
        }
    }
}
