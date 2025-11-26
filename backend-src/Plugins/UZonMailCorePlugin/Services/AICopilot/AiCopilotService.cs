using Microsoft.Extensions.AI;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.AICopilot
{
    public class AiCopilotService(SqlContext db, AIClientsManger aIClients) : IScopedService
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
                ?? throw new Exception("AI Copilot is not configured properly.");

            var response = await chatClient.GetResponseAsync(messages);
            return response.Messages.Last().Text;
        }
    }
}
