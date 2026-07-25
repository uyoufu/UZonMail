using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UzonMail.Utils.Web.Configs;

namespace UzonMailDotNET.Test.UzonMail.Utils;

[TestClass]
public sealed class OptionsInjectionExtensionsTests
{
    [TestMethod]
    public void AddAllOptions_DiscoversAndBindsMarkedOptions()
    {
        var configuration = new ConfigurationManager
        {
            ["Automatic:Value"] = "default-section",
            ["Features:Mail:Value"] = "nested-section",
        };
        var services = new ServiceCollection();

        services.AddAllOptions(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.AreEqual(
            "default-section",
            provider.GetRequiredService<IOptions<AutomaticOptions>>().Value.Value
        );
        Assert.AreEqual(
            "nested-section",
            provider.GetRequiredService<IOptions<NestedOptions>>().Value.Value
        );
    }

    [TestMethod]
    public void AddAllOptions_IsIdempotent()
    {
        var configuration = new ConfigurationManager();
        var services = new ServiceCollection();

        services.AddAllOptions(configuration);
        services.AddAllOptions(configuration);

        Assert.AreEqual(
            1,
            services.Count(descriptor =>
                descriptor.ServiceType == typeof(IConfigureOptions<AutomaticOptions>)
            )
        );
        Assert.AreEqual(
            1,
            services.Count(descriptor =>
                descriptor.ServiceType == typeof(IOptionsChangeTokenSource<AutomaticOptions>)
            )
        );
    }

    [TestMethod]
    public void ConfigurationReload_UpdatesMonitorAndNewSnapshotOnly()
    {
        var configuration = new ConfigurationManager { ["Reloadable:Value"] = "before" };
        var services = new ServiceCollection();
        services.AddAllOptions(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ReloadableOptions>>();
        var monitor = provider.GetRequiredService<IOptionsMonitor<ReloadableOptions>>();
        using var originalScope = provider.CreateScope();
        var originalSnapshot = originalScope.ServiceProvider.GetRequiredService<
            IOptionsSnapshot<ReloadableOptions>
        >();

        Assert.AreEqual("before", options.Value.Value);
        Assert.AreEqual("before", monitor.CurrentValue.Value);
        Assert.AreEqual("before", originalSnapshot.Value.Value);

        configuration["Reloadable:Value"] = "after";
        ((IConfigurationRoot)configuration).Reload();

        Assert.AreEqual("before", options.Value.Value);
        Assert.AreEqual("after", monitor.CurrentValue.Value);
        Assert.AreEqual("before", originalSnapshot.Value.Value);
        using var updatedScope = provider.CreateScope();
        var updatedSnapshot = updatedScope.ServiceProvider.GetRequiredService<
            IOptionsSnapshot<ReloadableOptions>
        >();
        Assert.AreEqual("after", updatedSnapshot.Value.Value);
    }

    public sealed class AutomaticOptions : IAppOptions
    {
        public string Value { get; set; } = string.Empty;
    }

    [OptionName("Features.Mail")]
    public sealed class NestedOptions : IAppOptions
    {
        public string Value { get; set; } = string.Empty;
    }

    public sealed class ReloadableOptions : IAppOptions
    {
        public string Value { get; set; } = string.Empty;
    }
}
