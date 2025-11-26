using UZonMail.Core.Utils.Cache;

namespace UZonMail.Core.Services.Settings.Model
{
    public enum AIProviderType
    {
        OpenAI = 1
    }

    public class AICopilotSetting : BaseSettingModel
    {
        public AIProviderType ProviderType { get; set; } = AIProviderType.OpenAI;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; }
        public long MaxTokens { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Endpoint)
                && !string.IsNullOrEmpty(ApiKey)
                && !string.IsNullOrEmpty(Model);
        }

        protected override void ReadValuesFromJsons()
        {
            ProviderType = (AIProviderType)GetIntValue(
                nameof(ProviderType),
                (int)AIProviderType.OpenAI
            );
            Endpoint = GetStringValue(nameof(Endpoint), string.Empty);
            ApiKey = GetStringValue(nameof(ApiKey), string.Empty);
            Model = GetStringValue(nameof(Model), string.Empty);
            MaxTokens = GetLongValue(nameof(MaxTokens), 2048);
        }
    }
}
