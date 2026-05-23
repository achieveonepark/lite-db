using System;

namespace SQLite
{
    public class SQLiteException : Exception
    {
        protected SQLiteException(SQLite3.Result result, string message)
            : base(message)
        {
            Result = result;
        }

        public SQLite3.Result Result { get; }

        public static SQLiteException New(SQLite3.Result result, string message)
        {
            return new SQLiteException(result, message);
        }
    }
}
