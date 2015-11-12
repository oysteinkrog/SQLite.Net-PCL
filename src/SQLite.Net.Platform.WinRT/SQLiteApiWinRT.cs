using System;
using SQLite.Net.Interop;
using System.Runtime.InteropServices;
using System.Text;
using Sqlite3DatabaseHandle = System.IntPtr;
using Sqlite3Statement = System.IntPtr;
using SQLite.Net.Platform.Shared.Common;
using SQLite.Net.Platform.Shared.Interop;

namespace SQLite.Net.Platform.WinRT
{
    public class SQLiteApiWinRT : BaseSQLiteApiExt
    {
        public SQLiteApiWinRT()
        {
            SQLite3.sqlite3_win32_set_directory(/*temp directory type*/2, Windows.Storage.ApplicationData.Current.TemporaryFolder.Path);
        }
    }
}