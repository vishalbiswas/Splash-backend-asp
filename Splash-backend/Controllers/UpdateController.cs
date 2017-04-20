using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Splash_backend
{
    [Route("update/{uid}")]
    public class UpdateController
    {
        [HttpPost]
        public ObjectResult Post(long uid, [FromQuery]string fname, [FromQuery]string lname, IFormFile profpic)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            SqlCommand command;
            if (profpic != null)
            {
                command = new SqlCommand("UPDATE users SET users.fname=@fname, users.lname=@lname, users.profpic=@profpic, users.picsize=@picsize WHERE users.uid=@uid");
                command.Parameters.Add("fname", SqlDbType.VarChar).Value = fname;
                command.Parameters.Add("lname", SqlDbType.VarChar).Value = lname;
                command.Parameters.Add("profpic", SqlDbType.Binary).Value = new BinaryReader(profpic.OpenReadStream()).ReadBytes(Convert.ToInt32(profpic.Length));
                command.Parameters.Add("picsize", SqlDbType.BigInt).Value = profpic.Length;
                command.Parameters.Add("uid", SqlDbType.BigInt).Value = uid;
            }
            else
            {
                command = new SqlCommand("UPDATE users SET users.fname = @fname, users.lname=@lname WHERE users.uid=@uid");
                command.Parameters.Add("fname", SqlDbType.VarChar).Value = fname;
                command.Parameters.Add("lname", SqlDbType.VarChar).Value = lname;
                command.Parameters.Add("uid", SqlDbType.BigInt).Value = uid;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            command.Connection = con;
            if (command.ExecuteNonQuery() == 1)
            {
                response.Add("status", 0);
                response.Add("msg", "Update success");
            }
            else
            {
                response.Add("status", 1);
                response.Add("msg", "Internal error");
            }
            return new ObjectResult(response);
        }
    }
}