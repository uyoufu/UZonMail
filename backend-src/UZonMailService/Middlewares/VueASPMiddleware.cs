using Microsoft.AspNetCore.Builder;

namespace UZonMailService.Middlewares
{
    public static class VueASPMiddlewareExtension
    {
        /// <summary>
        /// 配置 VueASP
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseVueASP(this IApplicationBuilder app)
        {
            var vueMiddleware = new VueASPMiddleware(null);
            // 不满足条件时，不启用中间件
            if (!vueMiddleware.IsValid) return app;

            return app.Use(vueMiddleware.Invoke);
        }
    }

    /// <summary>
    /// vue 单页应用中间件
    /// 将请求重定向到 index.html
    /// </summary>
    public class VueASPMiddleware
    {
        private readonly string _indexFilePath;
        private List<string> _existNames = [];

        public bool IsValid { get; private set; }
        public VueASPMiddleware(string wwwrootPath)
        {
            if (string.IsNullOrEmpty(wwwrootPath))
                wwwrootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

            _indexFilePath = Path.Combine(wwwrootPath, "index.html");
            // 若不存在，则不启用中间件
            IsValid = File.Exists(_indexFilePath);
            if (!IsValid) return;

            if (!Directory.Exists(wwwrootPath)) return;

            var files = Directory.GetFiles(wwwrootPath).Select(x => Path.GetFileName(x)).ToList();
            var dirs = Directory.GetDirectories(wwwrootPath).Select(x => Path.GetFileName(x)).ToList();
            _existNames.AddRange(files);
            _existNames.AddRange(dirs);
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            await next();

            // 找到的，直接返回
            if (context.Response.StatusCode != 404) return;

            var requestPath = context.Request.Path.Value;
            if (string.IsNullOrEmpty(requestPath) || requestPath.StartsWith("/api"))
            {
                return;
            }

            var firstPath = requestPath.Trim('/').Split('/').FirstOrDefault();
            if (string.IsNullOrEmpty(firstPath) || _existNames.Contains(firstPath))
            {
                return;
            }
            await context.Response.SendFileAsync(_indexFilePath);
        }
    }
}
