namespace Dialogues.Runtime
{
    public class LocalizationDataAdapter : ILocalizationService
    {
        private readonly LocalizationData _data;

        public LocalizationDataAdapter(LocalizationData data)
        {
            _data = data;
        }

        public string CurrentLanguage => _data.CurrentLanguage;
    }
}