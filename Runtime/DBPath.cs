using UnityEngine;

namespace Achieve.Database
{
    public static class DBPath
    {
        public static string DB_PATH => $"{Application.persistentDataPath}/data"; 
        public static string DB_FILE_PATH => $"{DB_PATH}/file.db"; 
        public static string ASSETS_FILE_PATH => $"{Application.streamingAssetsPath}/data/file.db"; 
        public static string LOCAL_FILE_PATH => $"{Application.persistentDataPath}/data/localdatasqlite.db"; 
    }
}