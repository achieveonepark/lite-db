#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Achieve.Database.Editor.CodeGeneration;
using Gilzoide.SqliteAsset.Csv;
using SQLite;
using UnityEditor;
using UnityEngine;

namespace Achieve.Database.Editor
{
    public class CsvImporterWindow : EditorWindow
    {
        private static CsvImporterWindow _instance;
        private string _csvPath;
        private DefaultAsset _dbAsset;
        private string _tableName;
        private const string NameSpace = "Achieve.Database.DataModel";
        
        private List<string> _csvHeaders = new List<string>();
        private List<List<string>> _csvRows = new List<List<string>>();
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            ValidateAssets();
            if (!string.IsNullOrEmpty(_csvPath))
            {
                _tableName = Path.GetFileNameWithoutExtension(_csvPath);
            }
            UpdateCsvPreview();
        }

        private void OnGUI()
        {
            DrawFilePath();
            DrawCsvContent();
            DrawButtons();
        }

        [MenuItem("GameFramework/Data/CsvImporter")]
        private static void ShowWindow()
        {
            _instance = GetWindow<CsvImporterWindow>("CSV Importer");
            _instance.minSize = new Vector2(600, 500);
        }

        private void DrawFilePath()
        {
            ValidateAssets();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("SQLite DB", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                _dbAsset = (DefaultAsset)EditorGUILayout.ObjectField("Database Asset", _dbAsset, typeof(DefaultAsset), false);
                if (GUILayout.Button("Create", GUILayout.Width(60)))
                {
                    var path = EditorUtility.SaveFilePanel("Save new DB file", Application.dataPath, "NewDatabase", "db");
                    if (!string.IsNullOrEmpty(path))
                    {
                        File.Create(path).Close();
                        AssetDatabase.Refresh();
                        // Convert absolute path to a relative asset path
                        var assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                        _dbAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetPath);
                    }
                }
            }


            EditorGUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("CSV File Path", GUILayout.Width(150));

                GUI.enabled = false;
                EditorGUILayout.TextField(_csvPath.Replace(Application.dataPath, ""));
                GUI.enabled = true;

                if (GUILayout.Button("Select"))
                {
                    var newPath = EditorUtility.OpenFilePanel("CSV File", Application.dataPath, "csv");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        _csvPath = newPath;
                        _tableName = Path.GetFileNameWithoutExtension(_csvPath);
                        UpdateCsvPreview();
                    }
                }
            }

            GUI.enabled = false;
            EditorGUILayout.TextField("Table Name", _tableName);
            GUI.enabled = true;
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString("csvAssetPath", _csvPath);
                SaveAssetPath("dbAssetPath", _dbAsset);
            }
        }

        private void DrawCsvContent()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("CSV 데이터 미리보기", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));

            if (_csvHeaders.Count > 0)
            {
                // Draw Header
                EditorGUILayout.BeginHorizontal();
                foreach (var header in _csvHeaders)
                {
                    EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


                // Draw Rows
                foreach (var row in _csvRows)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < row.Count; i++)
                    {
                        EditorGUILayout.LabelField(row[i]);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUI.enabled = string.IsNullOrEmpty(_csvPath) is false && _dbAsset != null &&
                          string.IsNullOrEmpty(_tableName) is false;
            if (GUILayout.Button("Insert!"))
            {
                var reader = File.OpenText(_csvPath);
                var dbAssetPath = AssetDatabase.GetAssetPath(_dbAsset);
                var db = new SQLiteConnection(dbAssetPath);
                db.ImportCsvToTable(_tableName, reader);

                EditorUtility.DisplayDialog("CSV Importer", $"{_tableName}데이터를 임포트했어요.", "OK");
            }

            if (GUILayout.Button("Generate C# Class"))
            {
                GenerateCSharpClass();
            }

            GUI.enabled = true;
        }

        private void GenerateCSharpClass()
        {
            if (string.IsNullOrEmpty(_tableName) || string.IsNullOrEmpty(_csvPath))
            {
                EditorUtility.DisplayDialog("Error", "Table Name and a valid CSV file are required.", "OK");
                return;
            }

            try
            {
                var className = _tableName;
                var columnInfo = CSharpCodeGenerator.InferColumnTypes(_csvPath);
                var classString = CSharpCodeGenerator.GenerateClassString(className, columnInfo, NameSpace);

                var directoryPath = Path.Combine(Application.dataPath, "Runtime", "DataModel");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, $"{className}.cs");
                File.WriteAllText(filePath, classString);
                
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success", $"'{className}.cs' has been generated successfully.", "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        
        private void SaveAssetPath(string key, Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            EditorPrefs.SetString(key, path);
        }

        private void ValidateAssets()
        {
            _csvPath ??= EditorPrefs.GetString("csvAssetPath", "");
            _dbAsset ??= AssetDatabase.LoadAssetAtPath<DefaultAsset>(EditorPrefs.GetString("dbAssetPath", ""));
        }

        private void UpdateCsvPreview()
        {
            _csvHeaders.Clear();
            _csvRows.Clear();

            if (string.IsNullOrEmpty(_csvPath) || !File.Exists(_csvPath))
            {
                return;
            }

            using (var streamReader = new StreamReader(_csvPath))
            {
                var allRows = ReadAllRows(streamReader).ToList();
                if (allRows.Count > 0)
                {
                    _csvHeaders.AddRange(allRows[0]);
                    _csvRows.AddRange(allRows.Skip(1).Take(5));
                }
            }
        }

        private IEnumerable<List<string>> ReadAllRows(TextReader textReader)
        {
            var currentRow = new List<string>();
            foreach (string field in CsvReader.ParseStream(textReader))
            {
                if (field == null)
                {
                    yield return currentRow;
                    currentRow = new List<string>();
                }
                else
                {
                    currentRow.Add(field);
                }
            }
        }
    }
}
#endif