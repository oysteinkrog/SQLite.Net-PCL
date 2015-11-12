using System;
using System.Runtime.InteropServices;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Shared.Common;
using SQLite.Net.Platform.Shared.Interop;

namespace SQLite.Net.Platform.Win32
{
    public class SQLiteApiWin32 : BaseSQLiteApiExt
    {
        public SQLiteApiWin32(string nativeInteropSearchPath = null)
        {
            if (nativeInteropSearchPath  != null)
                SQLite3Configuration.NativeInteropSearchPath = nativeInteropSearchPath;
        }        
    }
}
