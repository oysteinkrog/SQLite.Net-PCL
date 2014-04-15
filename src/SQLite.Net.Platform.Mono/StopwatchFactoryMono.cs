using System.Diagnostics;
using SQLite.Net.Interop;

namespace SQLite.Net.Platform.Mono
{
    public class StopwatchFactoryMono : IStopwatchFactory
    {
        public IStopwatch Create()
        {
            return new StopwatchMono();
        }

        private class StopwatchMono : IStopwatch
        {
            private readonly Stopwatch _stopWatch;

            public StopwatchMono()
            {
                _stopWatch = new Stopwatch();
            }

            public void Stop()
            {
                _stopWatch.Stop();
            }

            public void Reset()
            {
                _stopWatch.Reset();
            }

            public void Start()
            {
                _stopWatch.Start();
            }

            public long ElapsedMilliseconds
            {
                get { return _stopWatch.ElapsedMilliseconds; }
            }
        }
    }
}
