using System;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using Splash_backend.Models;

namespace Splash_backend
{
    [Route("update")]
    public class UpdateController
    {
        [HttpPost]
        public Dictionary<string, object> Post([FromForm]string sessionid, [FromForm]string fname, [FromForm]string lname, [FromForm]string email, [FromForm]string password, [FromForm]long profpic)
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
            SqlCommand command = new SqlCommand();
            string queryUpperHalf = "UPDATE users SET users.fname=@fname, users.lname=@lname, users.profpic=@profpic";
            if (fname == null)
            {
                command.Parameters.AddWithValue("fname", "");
            }
            else
            {
                command.Parameters.AddWithValue("fname", fname);
            }
            if (lname == null)
            {
                command.Parameters.AddWithValue("lname", "");
            }
            else
            {
                command.Parameters.AddWithValue("lname", lname);
            }
            if (profpic >= 0)
            {
                command.Parameters.AddWithValue("profpic", profpic);
            }
            else
            {
                command.Parameters.AddWithValue("profpic", DBNull.Value);
            }
            if (email != null)
            {
                queryUpperHalf += ", users.email=@email";
                command.Parameters.AddWithValue("email", email);
            }
            if (password != null)
            {
                queryUpperHalf += ", users.password=@password";
                command.Parameters.AddWithValue("password", password);
            }
            command.Parameters.AddWithValue("uid", user.uid);
            command.CommandText = queryUpperHalf + " WHERE uid=@uid;";
            command.Connection = con;
            con.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                response.Add("status", 0);
                response.Add("msg", "Update success");
                response.Add("fname", fname);
                response.Add("lname", lname);
                if (profpic >= 0) {
                    response.Add("profpic", profpic);
                }
                if (email != null)
                {
                    response.Add("email", email);
                }
            }
            else
            {
                response.Add("status", 1);
                response.Add("msg", "Internal error");
            }
            return response;
        }
    }
}