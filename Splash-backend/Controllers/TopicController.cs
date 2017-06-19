using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Splash_backend
{
	[Route("topics")]
	public class TopicController {
		struct Topic {
			public int topicid;
			public string name;
            public bool locked;
            public bool hidden;
		}

		[HttpGet]
		public ObjectResult Get() {
			SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT topics.topicid, topics.name, topics.locked, topics.hidden FROM topics", con);
            SqlDataReader reader = command.ExecuteReader();
            List<Topic> topics = new List<Topic>();

			while(reader.Read()) {
				topics.Add(new Topic() { topicid = reader.GetInt32(0), name = reader.GetString(1), locked = reader.GetBoolean(2), hidden = reader.GetBoolean(3)});
			}
			reader.Dispose();
			con.Close();

			return new ObjectResult(topics);
		}

		[HttpGet("{topicid}")]
		public ObjectResult Get(int topicid) {
			SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT topics.topicid, topics.name, topics.locked, topics.hidden FROM topics FROM topics WHERE topicid =" + topicid, con);
            SqlDataReader reader = command.ExecuteReader();
            Dictionary<string, object> response = new Dictionary<string, object>();

			if (reader.HasRows) {
				reader.Read();
				response.Add("topicid", reader.GetInt32(0));
				response.Add("name", reader.GetString(1));
                response.Add("locked", reader.GetBoolean(2));
                response.Add("hidden", reader.GetBoolean(3));
			}
			reader.Dispose();
			con.Close();

			return new ObjectResult(response);
		}

        internal static bool IsLocked(int topicid)
        {
            bool result = true;
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            con.Open();
            SqlCommand command = new SqlCommand("SELECT topics.locked FROM topics WHERE topicid = " + topicid, con);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                result = reader.GetBoolean(0);
            }
            reader.Dispose();
            con.Close();
            return result;
        }
	}
}