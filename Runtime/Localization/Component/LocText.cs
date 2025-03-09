using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Achieve.Database.Localization
{
    public class LocText : MonoBehaviour
    {
        public int Key;
        private TMP_Text text;

        private void Awake()
        {
            TryGetComponent<TMP_Text>(out var text);

            if (text == null)
            {
                throw new NullReferenceException($"이 컴포넌트에 TMP_Text가 포함되어있지 않습니다. {this.gameObject.name}");
            }
            
            if (Key != 0)
            {
                SetText(Key);
            }
        }

        public void SetText(int key, SystemLanguage language = SystemLanguage.Unknown)
        {
            Key = key;
            text.text = LocalizationManager.GetString(key, language);
        }
    }
}