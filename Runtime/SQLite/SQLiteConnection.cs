using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SQLite
{
    public sealed class SQLiteConnection : IDisposable
    {
        private static readonly SQLiteOpenFlags DefaultOpenFlags =
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;

        private readonly Dictionary<Type, TypeMap> _maps = new Dictionary<Type, TypeMap>();
        private IntPtr _handle;
        private bool _disposed;

        public SQLiteConnection(string databasePath)
            : this(databasePath, DefaultOpenFlags)
        {
        }

        public SQLiteConnection(string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true)
        {
            DatabasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
            StoreDateTimeAsTicks = storeDateTimeAsTicks;

            var result = SQLiteNative.Open(databasePath, out _handle, openFlags);
            if (result != SQLite3.Result.OK)
            {
                var message = _handle == IntPtr.Zero ? result.ToString() : SQLiteNative.GetErrorMessage(_handle);
                if (_handle != IntPtr.Zero)
                {
                    SQLiteNative.Close(_handle);
                    _handle = IntPtr.Zero;
                }

                throw SQLiteException.New(result, message);
            }

            SQLiteNative.BusyTimeout(_handle, 5000);
        }

        ~SQLiteConnection()
        {
            Dispose(false);
        }

        public string DatabasePath { get; }
        public bool StoreDateTimeAsTicks { get; }
        public IntPtr Handle => _handle;

        public static string Quote(string value)
        {
            if (value == null)
            {
                return "NULL";
            }

            return "'" + value.Replace("'", "''") + "'";
        }

        public static string QuoteIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Identifier cannot be empty.", nameof(identifier));
            }

            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        public T Get<T>(object primaryKey) where T : new()
        {
            EnsureNotDisposed();

            var map = GetMap(typeof(T));
            if (map.PrimaryKey == null)
            {
                throw new InvalidOperationException($"{typeof(T).Name} does not define a primary key.");
            }

            var query =
                $"SELECT * FROM {QuoteIdentifier(map.TableName)} WHERE {QuoteIdentifier(map.PrimaryKey.ColumnName)} = ? LIMIT 1";
            var results = Query<T>(query, primaryKey);
            if (results.Count == 0)
            {
                throw new InvalidOperationException("Sequence contains no elements.");
            }

            return results[0];
        }

        public List<T> Query<T>(string query, params object[] args) where T : new()
        {
            EnsureNotDisposed();

            using (var statement = Prepare(query, args))
            {
                var rows = new List<T>();
                while (true)
                {
                    var result = SQLiteNative.Step(statement.Handle);
                    if (result == SQLite3.Result.Row)
                    {
                        rows.Add(ReadRow<T>(statement.Handle));
                        continue;
                    }

                    if (result == SQLite3.Result.Done)
                    {
                        return rows;
                    }

                    Throw(result);
                }
            }
        }

        public T ExecuteScalar<T>(string query, params object[] args)
        {
            EnsureNotDisposed();

            using (var statement = Prepare(query, args))
            {
                var result = SQLiteNative.Step(statement.Handle);
                if (result == SQLite3.Result.Done)
                {
                    return default;
                }

                if (result != SQLite3.Result.Row)
                {
                    Throw(result);
                }

                return (T)ConvertColumn(statement.Handle, 0, typeof(T));
            }
        }

        public int Execute(string query, params object[] args)
        {
            EnsureNotDisposed();

            using (var statement = Prepare(query, args))
            {
                var result = SQLiteNative.Step(statement.Handle);
                if (result != SQLite3.Result.Done && result != SQLite3.Result.Row)
                {
                    Throw(result);
                }

                return SQLiteNative.Changes(_handle);
            }
        }

        public void RunInTransaction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Execute("BEGIN IMMEDIATE TRANSACTION");
            try
            {
                action();
                Execute("COMMIT");
            }
            catch
            {
                Execute("ROLLBACK");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private SQLiteStatement Prepare(string query, object[] args)
        {
            var statement = new SQLiteStatement(this, query);
            statement.Bind(args ?? Array.Empty<object>());
            return statement;
        }

        private T ReadRow<T>(IntPtr statement) where T : new()
        {
            var targetType = typeof(T);
            if (IsScalarType(targetType) && SQLiteNative.ColumnCount(statement) == 1)
            {
                return (T)ConvertColumn(statement, 0, targetType);
            }

            var target = new T();
            var map = GetMap(targetType);
            var columnIndexes = ReadColumnIndexes(statement);

            foreach (var member in map.Members)
            {
                if (!columnIndexes.TryGetValue(member.ColumnName, out var columnIndex))
                {
                    continue;
                }

                var value = ConvertColumn(statement, columnIndex, member.MemberType);
                member.SetValue(target, value);
            }

            return target;
        }

        private Dictionary<string, int> ReadColumnIndexes(IntPtr statement)
        {
            var count = SQLiteNative.ColumnCount(statement);
            var indexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < count; i++)
            {
                indexes[SQLiteNative.ColumnName(statement, i)] = i;
            }

            return indexes;
        }

        private object ConvertColumn(IntPtr statement, int index, Type targetType)
        {
            var nullableType = Nullable.GetUnderlyingType(targetType);
            var nonNullableType = nullableType ?? targetType;

            if (SQLiteNative.ColumnType(statement, index) == SQLite3.ColumnType.Null)
            {
                return nullableType != null || !nonNullableType.IsValueType
                    ? null
                    : Activator.CreateInstance(nonNullableType);
            }

            if (nonNullableType == typeof(object))
            {
                return SQLiteNative.ColumnValue(statement, index);
            }

            if (nonNullableType == typeof(string))
            {
                return SQLiteNative.ColumnString(statement, index);
            }

            if (nonNullableType == typeof(bool))
            {
                return ReadBoolean(statement, index);
            }

            if (nonNullableType.IsEnum)
            {
                return ReadEnum(statement, index, nonNullableType);
            }

            if (nonNullableType == typeof(byte[]))
            {
                return SQLiteNative.ColumnBlob(statement, index);
            }

            if (nonNullableType == typeof(DateTime))
            {
                return ReadDateTime(statement, index);
            }

            if (nonNullableType == typeof(Guid))
            {
                return Guid.Parse(SQLiteNative.ColumnString(statement, index));
            }

            if (nonNullableType == typeof(decimal))
            {
                return Convert.ToDecimal(ReadNumericValue(statement, index), CultureInfo.InvariantCulture);
            }

            if (IsNumericType(nonNullableType))
            {
                return Convert.ChangeType(ReadNumericValue(statement, index), nonNullableType, CultureInfo.InvariantCulture);
            }

            throw new NotSupportedException($"Column conversion to {targetType.Name} is not supported.");
        }

        private object ReadNumericValue(IntPtr statement, int index)
        {
            var columnType = SQLiteNative.ColumnType(statement, index);
            if (columnType == SQLite3.ColumnType.Integer)
            {
                return SQLiteNative.ColumnInt64(statement, index);
            }

            if (columnType == SQLite3.ColumnType.Float)
            {
                return SQLiteNative.ColumnDouble(statement, index);
            }

            var text = SQLiteNative.ColumnString(statement, index);
            return double.Parse(text, CultureInfo.InvariantCulture);
        }

        private bool ReadBoolean(IntPtr statement, int index)
        {
            var columnType = SQLiteNative.ColumnType(statement, index);
            if (columnType == SQLite3.ColumnType.Integer)
            {
                return SQLiteNative.ColumnInt64(statement, index) != 0;
            }

            var text = SQLiteNative.ColumnString(statement, index);
            return bool.TryParse(text, out var value) ? value : text != "0";
        }

        private object ReadEnum(IntPtr statement, int index, Type enumType)
        {
            if (SQLiteNative.ColumnType(statement, index) == SQLite3.ColumnType.Text)
            {
                return Enum.Parse(enumType, SQLiteNative.ColumnString(statement, index), true);
            }

            return Enum.ToObject(enumType, SQLiteNative.ColumnInt64(statement, index));
        }

        private DateTime ReadDateTime(IntPtr statement, int index)
        {
            if (SQLiteNative.ColumnType(statement, index) == SQLite3.ColumnType.Integer)
            {
                return new DateTime(SQLiteNative.ColumnInt64(statement, index));
            }

            return DateTime.Parse(
                SQLiteNative.ColumnString(statement, index),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        private TypeMap GetMap(Type type)
        {
            if (_maps.TryGetValue(type, out var map))
            {
                return map;
            }

            map = TypeMap.Create(type);
            _maps[type] = map;
            return map;
        }

        private void Throw(SQLite3.Result result)
        {
            throw SQLiteException.New(result, SQLiteNative.GetErrorMessage(_handle));
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SQLiteConnection));
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (_handle != IntPtr.Zero)
            {
                SQLiteNative.Close(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }

        private static bool IsScalarType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive
                   || type.IsEnum
                   || type == typeof(string)
                   || type == typeof(decimal)
                   || type == typeof(DateTime)
                   || type == typeof(Guid)
                   || type == typeof(byte[]);
        }

        private static bool IsNumericType(Type type)
        {
            return type == typeof(byte)
                   || type == typeof(sbyte)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(long)
                   || type == typeof(ulong)
                   || type == typeof(float)
                   || type == typeof(double);
        }

        private sealed class TypeMap
        {
            private TypeMap(string tableName, List<MappedMember> members, MappedMember primaryKey)
            {
                TableName = tableName;
                Members = members;
                PrimaryKey = primaryKey;
            }

            public string TableName { get; }
            public List<MappedMember> Members { get; }
            public MappedMember PrimaryKey { get; }

            public static TypeMap Create(Type type)
            {
                var table = type.GetCustomAttribute<TableAttribute>();
                var members = new List<MappedMember>();

                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!property.CanRead || !property.CanWrite || property.GetCustomAttribute<IgnoreAttribute>() != null)
                    {
                        continue;
                    }

                    members.Add(MappedMember.Create(property));
                }

                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (field.IsInitOnly || field.GetCustomAttribute<IgnoreAttribute>() != null)
                    {
                        continue;
                    }

                    members.Add(MappedMember.Create(field));
                }

                var primaryKey = members.FirstOrDefault(member => member.IsPrimaryKey)
                                 ?? members.FirstOrDefault(member =>
                                     string.Equals(member.ColumnName, "Id", StringComparison.OrdinalIgnoreCase));

                return new TypeMap(table?.Name ?? type.Name, members, primaryKey);
            }
        }

        private sealed class MappedMember
        {
            private readonly PropertyInfo _property;
            private readonly FieldInfo _field;

            private MappedMember(PropertyInfo property, FieldInfo field, string columnName, Type memberType, bool isPrimaryKey)
            {
                _property = property;
                _field = field;
                ColumnName = columnName;
                MemberType = memberType;
                IsPrimaryKey = isPrimaryKey;
            }

            public string ColumnName { get; }
            public Type MemberType { get; }
            public bool IsPrimaryKey { get; }

            public static MappedMember Create(PropertyInfo property)
            {
                var column = property.GetCustomAttribute<ColumnAttribute>();
                return new MappedMember(
                    property,
                    null,
                    column?.Name ?? property.Name,
                    property.PropertyType,
                    property.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            }

            public static MappedMember Create(FieldInfo field)
            {
                var column = field.GetCustomAttribute<ColumnAttribute>();
                return new MappedMember(
                    null,
                    field,
                    column?.Name ?? field.Name,
                    field.FieldType,
                    field.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            }

            public void SetValue(object target, object value)
            {
                if (_property != null)
                {
                    _property.SetValue(target, value, null);
                    return;
                }

                _field.SetValue(target, value);
            }
        }
    }
}
