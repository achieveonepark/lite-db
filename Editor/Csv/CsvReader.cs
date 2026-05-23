using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Achieve.Database.Editor.Csv
{
    public static class CsvReader
    {
        public enum SeparatorChar
        {
            Comma,
            Semicolon,
            Tabs
        }

        public static IEnumerable<string> ParseStream(
            TextReader stream,
            SeparatorChar separator = SeparatorChar.Comma,
            int maxFieldSize = int.MaxValue)
        {
            foreach (var row in ReadRows(stream, separator, maxFieldSize))
            {
                foreach (var field in row)
                {
                    yield return field;
                }

                yield return null;
            }
        }

        public static IEnumerable<List<string>> ReadRows(
            TextReader stream,
            SeparatorChar separator = SeparatorChar.Comma,
            int maxFieldSize = int.MaxValue)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var row = new List<string>();
            var field = new StringBuilder();
            var separatorChar = GetSeparator(separator);
            var insideQuotes = false;
            var touchedRow = false;

            while (true)
            {
                var value = stream.Read();
                if (value < 0)
                {
                    if (insideQuotes)
                    {
                        throw new CsvException("Quoted field is missing a closing quote.");
                    }

                    if (touchedRow || field.Length > 0 || row.Count > 0)
                    {
                        row.Add(field.ToString());
                        yield return row;
                    }

                    yield break;
                }

                var c = (char)value;
                touchedRow = true;

                if (insideQuotes)
                {
                    if (c == '"')
                    {
                        if (stream.Peek() == '"')
                        {
                            stream.Read();
                            Append(field, '"', maxFieldSize);
                        }
                        else
                        {
                            insideQuotes = false;
                        }
                    }
                    else
                    {
                        Append(field, c, maxFieldSize);
                    }

                    continue;
                }

                if (c == '"')
                {
                    insideQuotes = true;
                    continue;
                }

                if (c == separatorChar)
                {
                    row.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && stream.Peek() == '\n')
                    {
                        stream.Read();
                    }

                    row.Add(field.ToString());
                    field.Clear();

                    if (!IsEmptyRow(row))
                    {
                        yield return row;
                    }

                    row = new List<string>();
                    touchedRow = false;
                    continue;
                }

                Append(field, c, maxFieldSize);
            }
        }

        private static void Append(StringBuilder builder, char value, int maxFieldSize)
        {
            if (builder.Length >= maxFieldSize)
            {
                throw new CsvException("Field size is greater than maximum allowed size.");
            }

            builder.Append(value);
        }

        private static bool IsEmptyRow(List<string> row)
        {
            for (var i = 0; i < row.Count; i++)
            {
                if (!string.IsNullOrEmpty(row[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static char GetSeparator(SeparatorChar separator)
        {
            switch (separator)
            {
                case SeparatorChar.Semicolon:
                    return ';';
                case SeparatorChar.Tabs:
                    return '\t';
                default:
                    return ',';
            }
        }
    }
}
