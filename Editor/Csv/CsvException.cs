using System;

namespace Achieve.Database.Editor.Csv
{
    public sealed class CsvException : Exception
    {
        public CsvException(string message)
            : base(message)
        {
        }
    }
}
