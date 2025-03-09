using UnityEngine;

namespace Achieve.Database.Localization
{
    public class LocalizationCollection
    {
        public LocalizedData[] LocalizedDatas { get; private set; }

        public LocalizedData GetLocalizedData(SystemLanguage language)
        {
            for (var i = 0; i < LocalizedDatas.Length; i++)
            {
                if (LocalizedDatas[i].Language == language)
                {
                    return LocalizedDatas[i];
                }
            }

            return LocalizedData.Empty;
        }
    }

    public struct LocalizedData
    {
        public SystemLanguage Language;
        public string Text;
        public static LocalizedData Empty = new() { Language = SystemLanguage.Unknown };
    }
}