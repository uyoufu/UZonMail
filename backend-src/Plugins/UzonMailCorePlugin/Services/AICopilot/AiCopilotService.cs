using Microsoft.Extensions.AI;
using UzonMail.DB.SQL;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.AICopilot
{
    public class AiCopilotService(
        SqlContext db,
        AIClientsManger aIClients,
        ILogger<AiCopilotService> logger
    ) : IScopedService
    {
        /// <summary>
        /// Ask AI Copilot once
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> AskOnceAsync(long userId, IEnumerable<ChatMessage> messages)
        {
            var chatClient =
                await aIClients.GetChatClient(db, userId)
                ?? throw new KnownException("AI Copilot is not configured properly.");

            var response = await chatClient.GetResponseAsync(messages);
            return response.Messages.Last().Text;
        }
    }
}
