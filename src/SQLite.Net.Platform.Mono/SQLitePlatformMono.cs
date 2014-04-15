using SQLite.Net.Interop;

namespace SQLite.Net.Platform.Mono
{
    public class SQLitePlatformMono : ISQLitePlatform
    {
        public SQLitePlatformMono()
        {
            SQLiteApi = new SQLiteApiMono();
            StopwatchFactory = new StopwatchFactoryMono();
            ReflectionService = new ReflectionServiceMono();
            VolatileService = new VolatileServiceMono();
        }

        public ISQLiteApi SQLiteApi { get; private set; }
        public IStopwatchFactory StopwatchFactory { get; private set; }
        public IReflectionService ReflectionService { get; private set; }
        public IVolatileService VolatileService { get; private set; }
    }
}
