namespace Dialogues.Runtime
{
    public class LocalizationData
    {
        public string CurrentLanguage { get; private set; }

        public LocalizationData(string defaultLang = "ru")
        {
            CurrentLanguage = defaultLang;
        }

        public void SetLanguage(string langCode)
        {
            CurrentLanguage = langCode;
        }
    }
}