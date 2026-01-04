#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gilzoide.SqliteAsset.Csv;

namespace Achieve.Database.Editor.CodeGeneration
{
    public static class CSharpCodeGenerator
    {
        public static List<(string name, string type)> InferColumnTypes(string filePath)
        {
            // Reconstruct rows from the flat stream of fields provided by CsvReader.ParseStream
            var allRows = new List<List<string>>();
            using (var reader = File.OpenText(filePath))
            {
                var currentRow = new List<string>();
                foreach (var field in CsvReader.ParseStream(reader))
                {
                    if (field == null)
                    {
                        if (currentRow.Count > 0)
                        {
                            allRows.Add(currentRow);
                            currentRow = new List<string>();
                        }
                    }
                    else
                    {
                        currentRow.Add(field);
                    }
                }
                if (currentRow.Count > 0)
                {
                    allRows.Add(currentRow);
                }
            }

            if (allRows.Count == 0)
            {
                return new List<(string, string)>();
            }

            var headers = allRows[0];
            var dataRows = allRows.Skip(1).ToList();
            
            var columnTypes = new string[headers.Count];
            for (int i = 0; i < columnTypes.Length; i++)
            {
                columnTypes[i] = "int"; // Start with the most specific type
            }

            foreach (var row in dataRows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i >= columnTypes.Length) continue;
                    if (columnTypes[i] == "string") continue;
                    
                    var value = row[i];
                    if (columnTypes[i] == "int")
                    {
                        if (!int.TryParse(value, out _))
                        {
                            columnTypes[i] = "float";
                        }
                    }
                    if (columnTypes[i] == "float")
                    {
                        if (!float.TryParse(value, out _))
                        {
                            columnTypes[i] = "string";
                        }
                    }
                }
            }
            
            var result = new List<(string, string)>();
            for(int i = 0; i < headers.Count; i++)
            {
                result.Add((SanitizeIdentifier(headers[i]), columnTypes[i]));
            }
            return result;
        }

        public static string GenerateClassString(string className, List<(string name, string type)> columnInfo, string nameSpace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using SQLite;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}");
            sb.AppendLine("{");
            sb.AppendLine($"    [Table(\"{className}\")]");
            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");

            foreach (var (name, type) in columnInfo)
            {
                if (name.ToLower() == "id")
                {
                    sb.AppendLine("        [PrimaryKey, AutoIncrement]");
                }
                sb.AppendLine($"        public {type} {name} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string SanitizeIdentifier(string name)
        {
            // Simple sanitization for property names
            var sanitized = name.Replace(" ", "").Replace("-", "_");
            // Ensure it's a valid C# identifier start char
            if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
            {
                sanitized = "_" + sanitized;
            }
            return sanitized;
        }
    }
}
#endif