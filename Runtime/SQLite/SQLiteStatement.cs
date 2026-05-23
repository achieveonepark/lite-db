using System;

namespace SQLite
{
    internal sealed class SQLiteStatement : IDisposable
    {
        private readonly SQLiteConnection _connection;

        public SQLiteStatement(SQLiteConnection connection, string query)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Query = query ?? throw new ArgumentNullException(nameof(query));

            var result = SQLiteNative.Prepare(_connection.Handle, Query, out var handle);
            if (result != SQLite3.Result.OK)
            {
                throw SQLiteException.New(result, SQLiteNative.GetErrorMessage(_connection.Handle));
            }

            Handle = handle;
        }

        public string Query { get; }
        public IntPtr Handle { get; private set; }

        public void Bind(object[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                SQLiteNative.Bind(Handle, i + 1, args[i]);
            }
        }

        public void Dispose()
        {
            if (Handle == IntPtr.Zero)
            {
                return;
            }

            SQLiteNative.Finalize(Handle);
            Handle = IntPtr.Zero;
        }
    }
}
