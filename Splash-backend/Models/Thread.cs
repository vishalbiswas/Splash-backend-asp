using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Splash_backend.Models
{
    public struct Thread
    {
        public long threadid;
        public string title;
        public string content;
        public long author;
        public long ctime;
        public long mtime;
        public int topicid;
        public long attachid;
    }
}
