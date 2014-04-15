using System.Threading;
using SQLite.Net.Interop;

namespace SQLite.Net.Platform.Mono
{
    public class VolatileServiceMono : IVolatileService
    {
        public void Write(ref int transactionDepth, int depth)
        {
			Thread.VolatileWrite(ref transactionDepth, depth);
        }
    }
}
