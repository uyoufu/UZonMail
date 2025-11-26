using System.Collections.Concurrent;
using Microsoft.Extensions.AI;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.AICopilot
{
    public class AIClientsManger : ISingletonService
    {
        private readonly ConcurrentDictionary<string, IChatClient> _aiClients = new();
    }
}
