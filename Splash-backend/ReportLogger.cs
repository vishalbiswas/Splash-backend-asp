using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Splash_backend
{
    public static class ReportLogger
    {
        internal const uint REPORT = 0; // this won't be used as it is the default
        internal const uint RELEASE = 1;
        internal const uint LOCK = 2;
        internal const uint HIDE = 3;
        internal const uint UNLOCK = 4;
        internal const uint UNHIDE = 5;
        
        internal static bool LogAction(string type, uint action, long id, long modid)
        {
            string tableName, columnName;
            switch (type)
            {
                case "thread":
                    tableName = "reports_threads";
                    columnName = "threadid";
                    break;
                case "comment":
                    tableName = "reports_comments";
                    columnName = "commentid";
                    break;
                case "user":
                    tableName = "reports_users";
                    columnName = "uid";
                    break;
                default:
                    return false;
            }
            SqlConnection con = new SqlConnection(Program.Configuration["connectionStrings:splashConString"]);
            SqlCommand command = new SqlCommand("INSERT INTO " + tableName + " (" + columnName + ", modid, action) VALUES (" + id +", " + modid + ", " + action + ");", con);
            con.Open();

            if (command.ExecuteNonQuery() > 0)
            {
                con.Close();
                return true;
            }

            con.Close();
            return false;
        }
    }
}
