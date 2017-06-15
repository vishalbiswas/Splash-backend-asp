using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IO;
using Splash_backend.Models;

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
            SqlCommand command = new SqlCommand("SELECT attachments.type, attachments.data, attachments.size, attachments.filename FROM attachments WHERE attachments.attachid = " + attachid + ";", con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                Response.ContentType = (string)reader["type"];
                string filename;
                if (reader.IsDBNull(3))
                {
                    filename = attachid.ToString();
                }
                else
                {
                    filename = (string)reader["filename"];
                }
                Response.Headers.Add("FileName", filename);
                Response.Body.Write((byte[])reader["data"], 0, Convert.ToInt32(reader["size"]));
            }
            reader.Dispose();
            con.Close();
        }
    }

    [Produces("application/json")]
    [Route("upload")]
    public class UploadController : Controller
    {
        [HttpPost]
        public Dictionary<string, object> Post(IFormFile attach, [FromForm]string sessionid)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (!Program.users.TryGetValue(sessionid, out User user))
            {
                response.Add("status", 1);
                response.Add("msg", "Invalid session");
                return response;
            }
            if (user.banned)
            {
                response.Add("status", 4);
                response.Add("msg", "You are banned from doing this");
                return response;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("INSERT INTO attachments (type, data, size, filename) OUTPUT INSERTED.attachid VALUES(@type, @data, @size, @filename);", con);
            command.Parameters.AddWithValue("type", attach.ContentType);
            command.Parameters.AddWithValue("data", new BinaryReader(attach.OpenReadStream()).ReadBytes(Convert.ToInt32(attach.Length)));
            command.Parameters.AddWithValue("size", attach.Length);
            command.Parameters.AddWithValue("filename", attach.FileName);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                response.Add("status", 0);
                response.Add("attachid", (long)reader["attachid"]);
            }
            else
            {
                response.Add("status", 2);
            }
            reader.Dispose();
            con.Close();
            return response;
        }
    }
}
