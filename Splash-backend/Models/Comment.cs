namespace Splash_backend.Models
{
    struct Comment
    {
        public long commentid;
        public long threadid;
        public string content;
        public long author;
        public long parent;
        public long ctime;
        public long mtime;
        public bool locked;
        public bool hidden;
        public int reported;
    }
}
