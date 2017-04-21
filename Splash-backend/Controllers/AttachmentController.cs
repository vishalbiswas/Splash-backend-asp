using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Splash_backend.Controllers
{
    [Route("attachment")]
    public class AttachmentController : Controller
    {
        [HttpGet("{attachid}")]
        public void Get(long attachid)
        {
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT attachments.image, attachments.size FROM attachments WHERE attachments.attachid = " + attachid + ";", con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                Response.Body.Write((byte[])reader["image"], 0, Convert.ToInt32(reader["size"]));
            }
            reader.Dispose();
            con.Close();
        }
    }
}
