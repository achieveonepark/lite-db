using System;
using System.IO;
using System.Linq;
using SQLite;

namespace Achieve.Database.Editor.Csv
{
    public static class SQLiteConnectionCsvExtensions
    {
        public static void ImportCsvToTable(
            this SQLiteConnection db,
            string tableName,
            TextReader csvStream,
            CsvReader.SeparatorChar separator = CsvReader.SeparatorChar.Comma,
            int maxFieldSize = int.MaxValue)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be empty.", nameof(tableName));
            }

            if (csvStream == null)
            {
                throw new ArgumentNullException(nameof(csvStream));
            }

            using (var rows = CsvReader.ReadRows(csvStream, separator, maxFieldSize).GetEnumerator())
            {
                if (!rows.MoveNext())
                {
                    throw new CsvException("CSV does not contain a header row.");
                }

                var headers = rows.Current;
                if (headers.Count == 0 || headers.Any(string.IsNullOrWhiteSpace))
                {
                    throw new CsvException("Header cannot have empty column names.");
                }

                var quotedTable = SQLiteConnection.QuoteIdentifier(tableName);
                var quotedColumns = headers.Select(SQLiteConnection.QuoteIdentifier).ToArray();
                var tableColumns = string.Join(", ", quotedColumns.Select(column => $"{column} TEXT"));
                var insertColumns = string.Join(", ", quotedColumns);
                var placeholders = string.Join(", ", quotedColumns.Select(_ => "?"));

                db.RunInTransaction(() =>
                {
                    db.Execute($"CREATE TABLE IF NOT EXISTS {quotedTable} ({tableColumns})");

                    var insert = $"INSERT INTO {quotedTable} ({insertColumns}) VALUES ({placeholders})";
                    while (rows.MoveNext())
                    {
                        var row = rows.Current;
                        if (row.Count != headers.Count)
                        {
                            throw new CsvException($"Row has {row.Count} fields, but header has {headers.Count}.");
                        }

                        db.Execute(insert, row.Cast<object>().ToArray());
                    }
                });
            }
        }
    }
}
