using System;
using UnityEngine;

namespace Achieve.Database.Localization
{
    public class LocalizationManager
    {
        public static event Action onChangeLanguage;
        
        private static SystemLanguage _currentLanguage;

        public static void SetLanguage(SystemLanguage language)
        {
            _currentLanguage = language;
            onChangeLanguage?.Invoke();
        }

        /// <summary>
        /// 지정한 언어의 Text를 가져옵니다.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string GetString(int key, SystemLanguage language = SystemLanguage.Unknown)
        {
            var data = GetData(key, language);
            return data.Text;
        }

        /// <summary>
        /// Key에 해당하는 모든 언어가 담겨있는 데이터를 가져옵니다.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static LocalizationCollection GetCollection(int key)
        {
            return LiteDB.Get<LocalizationCollection>(key);
        }
        
        /// <summary>
        /// 지정한 언어의 Localization Data 클래스를 가져옵니다.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static LocalizedData GetData(int key, SystemLanguage language = SystemLanguage.Unknown)
        {
            if (language == SystemLanguage.Unknown)
            {
                language = _currentLanguage;
            }
            
            var data = LiteDB.Get<LocalizationCollection>(key);
            var localization = data.GetLocalizedData(language);
            
            return localization;
        }
    }
}