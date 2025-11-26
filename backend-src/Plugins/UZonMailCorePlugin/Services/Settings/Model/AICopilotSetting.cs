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
        public string EndPoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; }
        public long MaxTokens { get; set; }

        protected override void ReadValuesFromJsons()
        {
            ProviderType = (AIProviderType)GetIntValue(
                nameof(ProviderType),
                (int)AIProviderType.OpenAI
            );
            EndPoint = GetStringValue(nameof(EndPoint), string.Empty);
            ApiKey = GetStringValue(nameof(ApiKey), string.Empty);
            Model = GetStringValue(nameof(Model), "");
            MaxTokens = GetLongValue(nameof(MaxTokens), 2048);
        }
    }
}
