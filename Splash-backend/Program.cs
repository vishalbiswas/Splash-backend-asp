using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Splash_backend.Models;

namespace Splash_backend
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public const string COMMENT_TEMP_DDL = "DECLARE @t table (commentid bigint, ctime datetime, mtime datetime, parent bigint default null);";

        public static Dictionary<string, User> users = new Dictionary<string, User>();
        public static Dictionary<long, List<string>> sessions = new Dictionary<long, List<string>>();

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseUrls("http://0.0.0.0:5000")
                .Build();

            host.Run();
        }

        public static long ToUnixTimestamp(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static DateTime FromJavaTimestamp(long timeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timeStamp).ToLocalTime();
        }
    }
}
