using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Splash_backend
{
	[Route("topics")]
	public class TopicController {
		struct topic {
			public int topicid;
			public string name;
		}


		[HttpGet]
		public ObjectResult Get() {
			SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT topics.topicid, topics.name FROM topics", con);
            SqlDataReader reader = command.ExecuteReader();
            List<topic> topics = new List<topic>();

			while(reader.Read()) {
				topics.Add(new topic() { topicid = reader.GetInt32(0), name = reader.GetString(1)});
			}
			reader.Dispose();
			con.Close();

			return new ObjectResult(topics);
		}

		[HttpGet("{topicid}")]
		public ObjectResult Get(int topicid) {
			SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT topics.topicid, topics.name FROM topics WHERE topicid =" + topicid, con);
            SqlDataReader reader = command.ExecuteReader();
            Dictionary<string, object> response = new Dictionary<string, object>();

			if (reader.HasRows) {
				reader.Read();
				response.Add("topicid", reader.GetInt32(0));
				response.Add("name", reader.GetString(1));
			}
			reader.Dispose();
			con.Close();

			return new ObjectResult(response);
		}
	}
}