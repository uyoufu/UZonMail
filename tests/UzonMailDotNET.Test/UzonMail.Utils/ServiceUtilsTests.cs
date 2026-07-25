using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UzonMail.Utils.Web.Service;

namespace UzonMailDotNET.Test.UzonMail.Utils;

/// <summary>
/// 验证基于标记接口的自动服务注册行为。
/// </summary>
[TestClass]
public sealed class ServiceUtilsTests
{
    [TestMethod]
    public void AddServices_PreservesExplicitSelfRegistration()
    {
        var services = new ServiceCollection();
        services.AddScoped<AutomaticallyRegisteredService, ExplicitServiceReplacement>();

        ServiceUtils.AddServices(services, typeof(ServiceUtilsTests).Assembly);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsInstanceOfType<ExplicitServiceReplacement>(
            scope.ServiceProvider.GetRequiredService<AutomaticallyRegisteredService>()
        );
    }

    [TestMethod]
    public void AddServices_IsIdempotentAndPreservesContractImplementations()
    {
        var services = new ServiceCollection();

        ServiceUtils.AddServices(services, typeof(ServiceUtilsTests).Assembly);
        ServiceUtils.AddServices(services, typeof(ServiceUtilsTests).Assembly);

        Assert.HasCount(
            1,
            services.Where(descriptor =>
                descriptor.ServiceType == typeof(AutomaticallyRegisteredService)
            )
        );
        Assert.HasCount(
            2,
            services.Where(descriptor => descriptor.ServiceType == typeof(ITestExtension))
        );
        Assert.HasCount(
            1,
            services.Where(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(TestHostedService)
            )
        );
    }

    [TestMethod]
    public void AddServices_DiscoversHostedServiceWithoutServiceMarker()
    {
        var services = new ServiceCollection();
        ServiceUtils.AddServices(services, typeof(ServiceUtilsTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>();

        Assert.IsTrue(hostedServices.Any(service => service is TestHostedService));
    }
}

/// <summary>
/// 用于验证自身类型自动注册的测试服务。
/// </summary>
public class AutomaticallyRegisteredService : IScopedService;

/// <summary>
/// 用于验证显式注册优先级的替代服务。
/// </summary>
public sealed class ExplicitServiceReplacement : AutomaticallyRegisteredService;

/// <summary>
/// 用于验证多实现集合注册的测试契约。
/// </summary>
public interface ITestExtension : IScopedService<ITestExtension>;

/// <summary>
/// 测试契约的第一个实现。
/// </summary>
public sealed class TestExtensionA : ITestExtension;

/// <summary>
/// 测试契约的第二个实现。
/// </summary>
public sealed class TestExtensionB : ITestExtension;

/// <summary>
/// 不实现 IService 的测试后台服务。
/// </summary>
public sealed class TestHostedService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
