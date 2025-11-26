using System.ClientModel;
using System.Collections.Concurrent;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using UZonMail.Core.Services.AICopilot.Model;
using UZonMail.Core.Services.Encrypt;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.AICopilot
{
    /// <summary>
    /// AI 客户端管理器
    /// </summary>
    public class AIClientsManger : ISingletonService
    {
        private readonly AppSettingsManager _settingsManager;
        private readonly IEncryptService _encryptService;

        public AIClientsManger(IEncryptService encryptService, AppSettingsManager settingsManager)
        {
            _encryptService = encryptService;
            _settingsManager = settingsManager;

            StartIdleClientsCleanup();
        }

        // 若长时间不使用，则移除该缓存
        private readonly ConcurrentDictionary<string, CacheChatClient> _aiClients = new();

        /// <summary>
        /// 获取聊天客户端
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userI"></param>
        /// <returns></returns>
        public async Task<IChatClient?> GetChatClient(SqlContext db, long userI)
        {
            // 获取匹配
            var aiSetting = await _settingsManager.GetSetting<AICopilotSetting>(db, userI);
            if (!aiSetting.IsValid())
                return null;

            // 获取缓存
            var key = $"{aiSetting.Endpoint}_${aiSetting.ApiKey}_${aiSetting.Model}";
            if (_aiClients.TryGetValue(key, out var value))
            {
                return value;
            }

            // 创建新的客户端
            var credentials = new ApiKeyCredential(
                _encryptService.DecryptPassword(aiSetting.ApiKey)
            );
            var options = new OpenAI.OpenAIClientOptions();
            if (!string.IsNullOrEmpty(aiSetting.Endpoint))
            {
                options.Endpoint = new Uri(aiSetting.Endpoint);
            }
            var chatClient = new ChatClient(aiSetting.Model, credentials, options).AsIChatClient();
            _aiClients.TryAdd(key, new CacheChatClient(chatClient));
            return chatClient;
        }

        private void StartIdleClientsCleanup()
        {
            Task.Run(async () =>
            {
                var timeout = TimeSpan.FromMinutes(30);
                while (true)
                {
                    foreach (var key in _aiClients.Keys)
                    {
                        if (_aiClients.TryGetValue(key, out var client))
                        {
                            if ((DateTimeOffset.UtcNow - client.LastUsedDate) > timeout)
                            {
                                _aiClients.TryRemove(key, out _);
                            }
                        }
                    }
                }
            });
        }
    }
}
