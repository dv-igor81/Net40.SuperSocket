using System;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket
{
    public static class Extensions
    {
        public static void DoNotAwait(this Task task)
        {
            
        }

        public static void DoNotAwait(this ValueTask task)
        {

        }
    }
}