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
        public string type;
        public string filename;
        public bool locked;
        public bool hidden;
        public bool needmod;
        public int reported;
    }
}
