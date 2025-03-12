using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Quartz;
using UZonMail.Utils.Helpers;
using UZonMailService.Middlewares;
using Microsoft.AspNetCore.HttpLogging;
using UZonMail.Utils.Web;
using UZonMail.Utils.Web.Token;
using Uamazing.Utils.Plugin;
using UZonMail.Utils.Web.Filters;
using UZonMail.DB.SQL;
using UZonMail.DB.MySql;
using UZonMail.DB.SqLite;
using Microsoft.Extensions.DependencyInjection.Extensions;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using UZonMail.Utils.Log;

// 修改当前目录
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// 生成默认的配置文件
var productConfig = "appsettings.Production.json";
if (!File.Exists(productConfig))
{
    File.WriteAllText(productConfig, "{\n}");
}

// 复制 quartz 数据库
var quartzDb = "data/db/quartz-sqlite.sqlite3";
if (!File.Exists(quartzDb))
{
    Directory.CreateDirectory(Path.GetDirectoryName(quartzDb));
    File.Copy("Quartz/quartz-sqlite.sqlite3", quartzDb);
}

var appOptions = new WebApplicationOptions
{
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory),
    WebRootPath = "wwwroot",
    Args = args
};
var builder = WebApplication.CreateBuilder(appOptions);
var services = builder.Services;

// 保证只有一个实例
// services.UseSingleApp();

// 日志
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddLog4Net();

    var logLevel = builder.Configuration.GetSection("Logging:LogLevel:Default").Get<LogLevel>();
    loggingBuilder.AttachLevelToLog4Net(logLevel.ToString());
});
// 添加 http 日志
services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestProperties
        | HttpLoggingFields.RequestHeaders
        | HttpLoggingFields.ResponseHeaders;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});
//// log4net: https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore/blob/develop/samples/Net8.0/WebApi/log4net.config
//// 参考：https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore/blob/develop/samples/Net8.0/WebApi/Program.cs
//builder.Logging.ClearProviders();
//builder.Logging.AddLog4Net();


// 添加 httpClient
services.AddHttpClient();

// Add services to the container.
var mvcBuilder = services.AddControllers(option =>
{
    // 添加全局异常处理
    option.Filters.Add(new KnownExceptionFilter());
    option.Filters.Add(new TokenExpiredFilter());
})
.AddNewtonsoftJson(x =>
{
    x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// 加载插件
var pluginLoader = new PluginLoader("Plugins");
pluginLoader.AddApplicationPart(mvcBuilder);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();

// 配置 swagger
services.AddSwaggerGen(new OpenApiInfo()
{
    Title = "UZonMail API",
    Contact = new OpenApiContact()
    {
        Name = "galens",
        Url = new Uri("https://galens.uamazing.cn"),
        Email = "260827400@qq.com"
    }
}, "UZonMailService.xml");

// 验证在 jwt 中实现
// 添加 signalR，还需要在 app 中使用 MapHub
// 参考: https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-8.0&tabs=visual-studio
services.AddSignalR();
// Change to use Name as the user identifier for SignalR
// WARNING: This requires that the source of your JWT token 
// ensures that the Name claim is unique!
// If the Name claim isn't unique, users could receive messages 
// intended for a different user!
// 不使用自定义的 IUserIdProvider，使用默认的，保证 token 的 claim 中包含 ClaimTypes.Name,
//builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

// 设置 hyphen-case 路由
services.SetupSlugifyCaseRoute();

// 注入数据库
services.AddSqlContext<SqlContext, MySqlContext, SqLiteContext>(builder.Configuration);

// 添加 HttpContextAccessor，以供 service 获取当前请求的用户信息
services.AddHttpContextAccessor();

// 定时任务
services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));
// if you are using persistent job store, you might want to alter some options
services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true; // default: false
    options.Scheduling.OverWriteExistingData = true; // default: true
});
// 注入到 ioc 中
services.AddQuartz();
// 启动服务
services.AddQuartzHostedService(
    q => q.WaitForJobsToComplete = false);

// 配置 jwt 验证
var tokenParams = new TokenParams();
builder.Configuration.GetSection("TokenParams").Bind(tokenParams);
services.AddJWTAuthentication(tokenParams.UniqueSecret);

// 配置接口鉴权策略
services.AddAuthorizationBuilder()
    // 超管
    .AddPolicy("RequireAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "admin"));

// 关闭参数自动检验
services.Configure<ApiBehaviorOptions>(o =>
{
    o.SuppressModelStateInvalidFilter = true;
});

// 跨域
services.AddCors(options =>
{
    var configuration = builder.Configuration;
    // 获取跨域配置
    string[]? corsConfig = configuration.GetSection("Cors").Get<string[]>();

    // 获取当前主机的地址
    var hostIPs = NetworkHelper.GetCurrentHostIPs();
    var port = configuration.GetSection("Http:Port").Get<int>();
    var hostUrls = hostIPs.Select(x => $"http://{x}:{port}").ToList();
    List<string> cors = [$"http://127.0.0.1:{port}", $"http://localhost:{port}", "http://localhost:9000"];
    cors.AddRange(hostUrls);

    if (corsConfig?.Length > 0) cors.AddRange(corsConfig);

    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins([.. cors])
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});

// 修改文件上传大小限制
services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
});

// 配置 Kestrel 服务器
// 默认监听地址通过 Urls 配置 
builder.WebHost.ConfigureKestrel(options =>
{
    bool listenAnyIP = builder.Configuration.GetSection("Http:ListenAnyIP").Get<bool>();
    int port = builder.Configuration.GetSection("Http:Port").Get<int>();
    if (listenAnyIP)
        options.ListenAnyIP(port);
    else
        options.ListenLocalhost(port);

    options.Limits.MaxRequestBodySize = int.MaxValue;
});

// 加载本机服务
services.AddServices();

// 加载插件服务
pluginLoader.UseServices(builder);

var app = builder.Build();

app.UseDefaultFiles();
// 设置网站的根目录
app.UseStaticFiles();

// 设置 public 目录为静态文件目录
var publicPath = Path.Combine(builder.Environment.ContentRootPath, "public");
Directory.CreateDirectory(publicPath);
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(publicPath),
    RequestPath = "/public"
});

// 跨域
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();

// vue 单页面应用中间件
app.UseVueASP();

// http 路由
app.MapControllers();

pluginLoader.UseApp(app);

app.Run();

