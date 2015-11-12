using System;
using System.Runtime.InteropServices;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Shared.Common;
using SQLite.Net.Platform.Shared.Interop;

namespace SQLite.Net.Platform.OSX
{
    public class SQLiteApiOSX : BaseSQLiteApiExt
    {
        public Result Open(byte[] filename, out IDbHandle db, int flags, IntPtr zvfs)
        {
            IntPtr dbPtr;
            Result r = SQLiteApiInternal.sqlite3_open_v2(filename, out dbPtr, flags, zvfs);
            db = new DbHandle(dbPtr);
            return r;
        }

        public ExtendedResult ExtendedErrCode(IDbHandle db)
        {
            var internalDbHandle = (DbHandle) db;
            return SQLiteApiInternal.sqlite3_extended_errcode(internalDbHandle.DbPtr);
        }

        public int LibVersionNumber()
        {
            return SQLiteApiInternal.sqlite3_libversion_number();
        }
        
        public string SourceID()
        {
			return Marshal.PtrToStringAuto(SQLiteApiInternal.sqlite3_sourceid());            
        }                

        public Result EnableLoadExtension(IDbHandle db, int onoff)
        {
            var internalDbHandle = (DbHandle) db;
            return SQLiteApiInternal.sqlite3_enable_load_extension(internalDbHandle.DbPtr, onoff);
        }

        public Result Close(IDbHandle db)
        {
            var internalDbHandle = (DbHandle) db;
            return SQLiteApiInternal.sqlite3_close(internalDbHandle.DbPtr);
        }

        public Result Initialize()
        {
            return SQLiteApiInternal.sqlite3_initialize();
        }
        public Result Shutdown()
        {
            return SQLiteApiInternal.sqlite3_shutdown();
        }

        public Result Config(ConfigOption option)
        {
            return SQLiteApiInternal.sqlite3_config(option);
        }


        public Result BusyTimeout(IDbHandle db, int milliseconds)
        {
            var internalDbHandle = (DbHandle) db;
            return SQLiteApiInternal.sqlite3_busy_timeout(internalDbHandle.DbPtr, milliseconds);
        }

        public int Changes(IDbHandle db)
        {
            var internalDbHandle = (DbHandle) db;
            return SQLiteApiInternal.sqlite3_changes(internalDbHandle.DbPtr);
        }

        public IDbStatement Prepare2(IDbHandle db, string query)
        {
            var internalDbHandle = (DbHandle) db;
            IntPtr stmt;
            Result r = SQLiteApiInternal.sqlite3_prepare_v2(internalDbHandle.DbPtr, query, query.Length, out stmt, IntPtr.Zero);
            if (r != Result.OK)
            {
                throw SQLiteException.New(r, Errmsg16(internalDbHandle));
            }
            return new DbStatement(stmt);
        }

        public Result Step(IDbStatement stmt)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_step(internalStmt.StmtPtr);
        }

        public Result Reset(IDbStatement stmt)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_reset(internalStmt.StmtPtr);
        }

        public Result Finalize(IDbStatement stmt)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_finalize(internalStmt.StmtPtr);
        }

        public long LastInsertRowid(IDbHandle db)
        {
            var internalDbHandle = (DbHandle) db;
            return SQLiteApiInternal.sqlite3_last_insert_rowid(internalDbHandle.DbPtr);
        }

        public string Errmsg16(IDbHandle db)
        {
            var internalDbHandle = (DbHandle) db;
            return Marshal.PtrToStringUni(SQLiteApiInternal.sqlite3_errmsg16(internalDbHandle.DbPtr));
        }

        public int BindParameterIndex(IDbStatement stmt, string name)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_bind_parameter_index(internalStmt.StmtPtr, name);
        }

        public int BindNull(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_bind_null(internalStmt.StmtPtr, index);
        }

        public int BindInt(IDbStatement stmt, int index, int val)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_bind_int(internalStmt.StmtPtr, index, val);
        }

        public int BindInt64(IDbStatement stmt, int index, long val)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_bind_int64(internalStmt.StmtPtr, index, val);
        }

        public int BindDouble(IDbStatement stmt, int index, double val)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_bind_double(internalStmt.StmtPtr, index, val);
        }

        public int BindText16(IDbStatement stmt, int index, string val, int n, IntPtr free)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_bind_text16(internalStmt.StmtPtr, index, val, n, free);
        }

        public int BindBlob(IDbStatement stmt, int index, byte[] val, int n, IntPtr free)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_bind_blob(internalStmt.StmtPtr, index, val, n, free);
        }

        public int ColumnCount(IDbStatement stmt)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_column_count(internalStmt.StmtPtr);
        }

        public string ColumnName16(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.ColumnName16(internalStmt.StmtPtr, index);
        }

        public ColType ColumnType(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_column_type(internalStmt.StmtPtr, index);
        }

        public int ColumnInt(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_column_int(internalStmt.StmtPtr, index);
        }

        public long ColumnInt64(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_column_int64(internalStmt.StmtPtr, index);
        }

        public double ColumnDouble(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_column_double(internalStmt.StmtPtr, index);
        }

        public string ColumnText16(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return Marshal.PtrToStringUni(SQLiteApiInternal.sqlite3_column_text16(internalStmt.StmtPtr, index));
        }

        public byte[] ColumnBlob(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.ColumnBlob(internalStmt.StmtPtr, index);
        }

        public int ColumnBytes(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.sqlite3_column_bytes(internalStmt.StmtPtr, index);
        }

        public byte[] ColumnByteArray(IDbStatement stmt, int index)
        {
            var internalStmt = (DbStatement) stmt;
            return SQLiteApiInternal.ColumnByteArray(internalStmt.StmtPtr, index);
        }
    }
}
