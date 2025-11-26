using Microsoft.Extensions.AI;

namespace UZonMail.Core.Services.AICopilot.Model
{
    public class CacheChatClient(IChatClient chatClient) : IChatClient
    {
        public DateTimeOffset LastUsedDate { get; private set; } = DateTimeOffset.UtcNow;

        public void Dispose()
        {
            chatClient.Dispose();
        }

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            LastUsedDate = DateTimeOffset.UtcNow;
            return await chatClient.GetResponseAsync(messages, options, cancellationToken);
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return chatClient.GetService(serviceType, serviceKey);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            LastUsedDate = DateTimeOffset.UtcNow;
            return chatClient.GetStreamingResponseAsync(messages, options, cancellationToken);
        }
    }
}
