using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Splash_backend.Models
{
    public struct Notification
    {
        public long notifyid;
        public long uid;
        public int code;
        public string custom;
        public bool done;
        public long commentid;
        public long threadid;
        public long actionuid;
    }
}
