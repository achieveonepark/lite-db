using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;
using UnityEngine.Networking;

namespace Achieve.Database
{
    /// <summary>
    /// db에 기록해둔 데이터를 쉽게 읽어오도록 도움을 주는 기능들을 제공합니다.
    /// </summary>
    public static class LiteDB
    {
        private static SQLiteConnection _db;

        private static bool _isInitialized => _db != null;

        /// <summary>
        /// 내부에서 사용 할 DB 객체를 생성합니다.
        /// </summary>
        /// <param name="path"></param>
        public static void Initialize(string path)
        {
            _db = new SQLiteConnection(path);
            Debug.Log("[LiteDB] Initialized");
        }

        public static async UniTask Initialize()
        {
            if (Directory.Exists(DBPath.DB_PATH) is false)
            {
                Directory.CreateDirectory(DBPath.DB_PATH);
            }

            await CopyDbFile();

            _db = new SQLiteConnection(DBPath.LOCAL_FILE_PATH);
            Debug.Log("[LiteDB] Initialized");
        }

        /// <summary>
        /// db에 Primary Key로 설정했던 Id를 입력하여 데이터를 가져옵니다.
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>(object id) where T : new()
        {
            if (_isInitialized is false)
            {
                return default;
            }

            return _db.Get<T>(id);
        }

        /// <summary>
        /// StartId ~ EndId 사이의 데이터들을 조회하여 List로 반환합니다.
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetList<T>(int startId, int endId) where T : IDataBase, new()
        {
            string query = $"SELECT * FROM {typeof(T).Name} WHERE Id >= {startId} AND Id <= {endId}";
            return _db.Query<T>(query);
        }

        /// <summary>
        /// 제너릭으로 추가한 테이블의 id가 존재하는지 확인한 후 결과를 반환합니다.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Exist<T>(int id) where T : IDataBase
        {
            string query = $"SELECT COUNT(1) FROM {typeof(T).Name} WHERE ID = ?";
            int count = (int)_db.ExecuteScalar<object>(query, id);
            return count > 0;
        }

        /// <summary>
        /// 데이터를 조회하여 값이 있다면 true와 out으로 데이터를 반환하고, 없으면 false와 null을 반환합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetValue<T>(int id, out T result) where T : IDataBase, new()
        {
            if (_isInitialized is false)
            {
                result = default;
                return false;
            }

            string query = $"SELECT COUNT(1) FROM {typeof(T).Name} WHERE ID = ?";
            int count = _db.ExecuteScalar<int>(query, id);
            if (count > 0)
            {
                result = Get<T>(id);
                return true;
            }

            result = default(T);
            return false;
        }

        private static async UniTask CopyDbFile()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                using var request = UnityWebRequest.Get(DBPath.ASSETS_FILE_PATH);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(DBPath.LOCAL_FILE_PATH, request.downloadHandler.data);
                }
                else
                {
                    Debug.LogError($"Failed to copy database: {request.error}");
                }
            }
            else
            {
                if (Directory.Exists(DBPath.LOCAL_FILE_PATH))
                {
                    Directory.CreateDirectory(DBPath.LOCAL_FILE_PATH);
                }
                
                // iOS, Windows, Mac은 바로 복사 가능
                File.Copy(DBPath.ASSETS_FILE_PATH, DBPath.LOCAL_FILE_PATH, true);
            }
        }
    }
}