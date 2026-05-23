using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SQLite
{
    public static class SQLite3
    {
        public enum Result
        {
            OK = 0,
            Error = 1,
            Internal = 2,
            Perm = 3,
            Abort = 4,
            Busy = 5,
            Locked = 6,
            NoMem = 7,
            ReadOnly = 8,
            Interrupt = 9,
            IOError = 10,
            Corrupt = 11,
            NotFound = 12,
            Full = 13,
            CantOpen = 14,
            Protocol = 15,
            Empty = 16,
            Schema = 17,
            TooBig = 18,
            Constraint = 19,
            Mismatch = 20,
            Misuse = 21,
            NoLFS = 22,
            Auth = 23,
            Format = 24,
            Range = 25,
            NotADb = 26,
            Notice = 27,
            Warning = 28,
            Row = 100,
            Done = 101
        }

        public enum ColumnType
        {
            Integer = 1,
            Float = 2,
            Text = 3,
            Blob = 4,
            Null = 5
        }
    }

    [Flags]
    public enum SQLiteOpenFlags
    {
        ReadOnly = 1,
        ReadWrite = 2,
        Create = 4,
        NoMutex = 0x8000,
        FullMutex = 0x10000,
        SharedCache = 0x20000,
        PrivateCache = 0x40000,
        ProtectionComplete = 0x00100000,
        ProtectionCompleteUnlessOpen = 0x00200000,
        ProtectionCompleteUntilFirstUserAuthentication = 0x00300000,
        ProtectionNone = 0x00400000
    }

    internal static class SQLiteNative
    {
#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_IOS || UNITY_TVOS || UNITY_VISIONOS)
        private const string LibraryPath = "__Internal";
#else
        private const string LibraryPath = "gilzoide-sqlite-net";
#endif

        private static readonly IntPtr Transient = new IntPtr(-1);

        public static SQLite3.Result Open(string path, out IntPtr db, SQLiteOpenFlags openFlags)
        {
            var bytes = ToNullTerminatedUtf8(path);
            return sqlite3_open_v2(bytes, out db, ToNativeOpenFlags(openFlags), IntPtr.Zero);
        }

        public static SQLite3.Result Close(IntPtr db)
        {
            return sqlite3_close_v2(db);
        }

        public static SQLite3.Result BusyTimeout(IntPtr db, int milliseconds)
        {
            return sqlite3_busy_timeout(db, milliseconds);
        }

        public static SQLite3.Result Prepare(IntPtr db, string query, out IntPtr statement)
        {
            var bytes = ToNullTerminatedUtf8(query);
            return sqlite3_prepare_v2(db, bytes, -1, out statement, IntPtr.Zero);
        }

        public static SQLite3.Result Step(IntPtr statement)
        {
            return sqlite3_step(statement);
        }

        public static SQLite3.Result Finalize(IntPtr statement)
        {
            return sqlite3_finalize(statement);
        }

        public static int Changes(IntPtr db)
        {
            return sqlite3_changes(db);
        }

        public static int ColumnCount(IntPtr statement)
        {
            return sqlite3_column_count(statement);
        }

        public static string ColumnName(IntPtr statement, int index)
        {
            return PtrToUtf8String(sqlite3_column_name(statement, index));
        }

        public static SQLite3.ColumnType ColumnType(IntPtr statement, int index)
        {
            return sqlite3_column_type(statement, index);
        }

        public static long ColumnInt64(IntPtr statement, int index)
        {
            return sqlite3_column_int64(statement, index);
        }

        public static double ColumnDouble(IntPtr statement, int index)
        {
            return sqlite3_column_double(statement, index);
        }

        public static string ColumnString(IntPtr statement, int index)
        {
            var pointer = sqlite3_column_text(statement, index);
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            return PtrToUtf8String(pointer, sqlite3_column_bytes(statement, index));
        }

        public static byte[] ColumnBlob(IntPtr statement, int index)
        {
            var size = sqlite3_column_bytes(statement, index);
            if (size <= 0)
            {
                return Array.Empty<byte>();
            }

            var pointer = sqlite3_column_blob(statement, index);
            if (pointer == IntPtr.Zero)
            {
                return Array.Empty<byte>();
            }

            var bytes = new byte[size];
            Marshal.Copy(pointer, bytes, 0, size);
            return bytes;
        }

        public static object ColumnValue(IntPtr statement, int index)
        {
            switch (ColumnType(statement, index))
            {
                case SQLite3.ColumnType.Integer:
                    var integer = ColumnInt64(statement, index);
                    return integer >= int.MinValue && integer <= int.MaxValue ? (object)(int)integer : integer;
                case SQLite3.ColumnType.Float:
                    return ColumnDouble(statement, index);
                case SQLite3.ColumnType.Text:
                    return ColumnString(statement, index);
                case SQLite3.ColumnType.Blob:
                    return ColumnBlob(statement, index);
                default:
                    return null;
            }
        }

        public static void Bind(IntPtr statement, int index, object value)
        {
            SQLite3.Result result;
            if (value == null)
            {
                result = sqlite3_bind_null(statement, index);
            }
            else if (value is bool boolValue)
            {
                result = sqlite3_bind_int(statement, index, boolValue ? 1 : 0);
            }
            else if (value is byte[] blobValue)
            {
                result = sqlite3_bind_blob(statement, index, blobValue, blobValue.Length, Transient);
            }
            else if (value is float || value is double || value is decimal)
            {
                result = sqlite3_bind_double(statement, index, Convert.ToDouble(value));
            }
            else if (value is Enum)
            {
                result = sqlite3_bind_int64(statement, index, Convert.ToInt64(value));
            }
            else if (value is byte || value is sbyte || value is short || value is ushort
                     || value is int || value is uint || value is long || value is ulong)
            {
                result = sqlite3_bind_int64(statement, index, Convert.ToInt64(value));
            }
            else if (value is DateTime dateTime)
            {
                var bytes = ToUtf8(dateTime.ToString("o"));
                result = sqlite3_bind_text(statement, index, bytes, bytes.Length, Transient);
            }
            else
            {
                var bytes = ToUtf8(Convert.ToString(value));
                result = sqlite3_bind_text(statement, index, bytes, bytes.Length, Transient);
            }

            if (result != SQLite3.Result.OK)
            {
                throw SQLiteException.New(result, $"Failed to bind parameter {index}.");
            }
        }

        public static string GetErrorMessage(IntPtr db)
        {
            return PtrToUtf8String(sqlite3_errmsg(db));
        }

        private static int ToNativeOpenFlags(SQLiteOpenFlags flags)
        {
            var nativeFlags = 0;
            if ((flags & SQLiteOpenFlags.ReadOnly) != 0)
            {
                nativeFlags |= (int)SQLiteOpenFlags.ReadOnly;
            }

            if ((flags & SQLiteOpenFlags.ReadWrite) != 0)
            {
                nativeFlags |= (int)SQLiteOpenFlags.ReadWrite;
            }

            if ((flags & SQLiteOpenFlags.Create) != 0)
            {
                nativeFlags |= (int)SQLiteOpenFlags.Create;
            }

            if ((flags & SQLiteOpenFlags.NoMutex) != 0)
            {
                nativeFlags |= (int)SQLiteOpenFlags.NoMutex;
            }

            if ((flags & SQLiteOpenFlags.FullMutex) != 0)
            {
                nativeFlags |= (int)SQLiteOpenFlags.FullMutex;
            }

            if ((flags & SQLiteOpenFlags.SharedCache) != 0)
            {
                nativeFlags |= (int)SQLiteOpenFlags.SharedCache;
            }

            if ((flags & SQLiteOpenFlags.PrivateCache) != 0)
            {
                nativeFlags |= (int)SQLiteOpenFlags.PrivateCache;
            }

            return nativeFlags == 0
                ? (int)(SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create)
                : nativeFlags;
        }

        private static byte[] ToUtf8(string text)
        {
            return Encoding.UTF8.GetBytes(text ?? string.Empty);
        }

        private static byte[] ToNullTerminatedUtf8(string text)
        {
            var textBytes = ToUtf8(text);
            var bytes = new byte[textBytes.Length + 1];
            Buffer.BlockCopy(textBytes, 0, bytes, 0, textBytes.Length);
            return bytes;
        }

        private static string PtrToUtf8String(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            var length = 0;
            while (Marshal.ReadByte(pointer, length) != 0)
            {
                length++;
            }

            return PtrToUtf8String(pointer, length);
        }

        private static string PtrToUtf8String(IntPtr pointer, int byteCount)
        {
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            if (byteCount <= 0)
            {
                return string.Empty;
            }

            var bytes = new byte[byteCount];
            Marshal.Copy(pointer, bytes, 0, byteCount);
            return Encoding.UTF8.GetString(bytes, 0, byteCount);
        }

        [DllImport(LibraryPath, EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_open_v2(byte[] filename, out IntPtr db, int flags, IntPtr zvfs);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_close_v2", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_close_v2(IntPtr db);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_busy_timeout", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_busy_timeout(IntPtr db, int milliseconds);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_prepare_v2(IntPtr db, byte[] sql, int numBytes, out IntPtr statement, IntPtr tail);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_step(IntPtr statement);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_finalize(IntPtr statement);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_changes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_changes(IntPtr db);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_column_count(IntPtr statement);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_column_name(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.ColumnType sqlite3_column_type(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern long sqlite3_column_int64(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double sqlite3_column_double(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_column_text(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_bytes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_column_bytes(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_column_blob(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_null", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_bind_null(IntPtr statement, int index);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_bind_int(IntPtr statement, int index, int value);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_bind_int64(IntPtr statement, int index, long value);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_bind_double(IntPtr statement, int index, double value);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_text", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_bind_text(IntPtr statement, int index, byte[] value, int length, IntPtr destructor);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_blob", CallingConvention = CallingConvention.Cdecl)]
        private static extern SQLite3.Result sqlite3_bind_blob(IntPtr statement, int index, byte[] value, int length, IntPtr destructor);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_errmsg", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_errmsg(IntPtr db);
    }
}
