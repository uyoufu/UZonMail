
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace UZonMail.Core.Services.HostedServices
{
    /// <summary>
    /// 设置前端的 BaseUrl
    /// </summary>
    public class SetupBaseUrlWhenStartp(IConfiguration configuration) : IHostedServiceStart
    {
        public int Order => 0;

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var frontConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app.config.json");
            if (!File.Exists(frontConfigPath)) return Task.CompletedTask;

            var baseUrl = configuration.GetValue<string>("BaseUrl");
            if(string.IsNullOrEmpty(baseUrl)) return Task.CompletedTask;

            // 读取为 json
            var jobject = JObject.Parse(File.ReadAllText(frontConfigPath));            
            jobject["baseUrl"] = baseUrl;
            // 保存文件
            File.WriteAllText(frontConfigPath, jobject.ToString());

            return Task.CompletedTask;
        }
    }
}
