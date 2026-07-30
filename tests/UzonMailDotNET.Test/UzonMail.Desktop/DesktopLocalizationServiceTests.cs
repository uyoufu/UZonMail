using System.Globalization;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UzonMailDesktop.Configuration;
using UzonMailDesktop.Localization;

namespace UzonMailDotNET.Test.UzonMail.Desktop;

/// <summary>
/// 验证桌面端语言解析、资源切换和生产配置持久化行为
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class DesktopLocalizationServiceTests
{
    private static CultureInfo? _originalDefaultCulture;
    private static CultureInfo? _originalDefaultUiCulture;

    [ClassInitialize]
    public static void CaptureDefaultCultures(TestContext testContext)
    {
        _originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        _originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
    }

    [TestCleanup]
    public void RestoreDefaultCultures()
    {
        CultureInfo.DefaultThreadCurrentCulture = _originalDefaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _originalDefaultUiCulture;
    }

    [TestMethod]
    public void Constructor_UsesConfiguredLocaleBeforeSystemLocale()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(
                Path.Combine(temporaryDirectory, "appsettings.Production.json"),
                "{\"Localization\":{\"Locale\":\"zh-CN\"}}"
            );
            var configuredLocale = new ConfigurationBuilder()
                .SetBasePath(temporaryDirectory)
                .AddJsonFile("appsettings.Production.json", optional: false, reloadOnChange: false)
                .Build()
                .GetSection(DesktopLocalizationOptions.SectionName)
                .Get<DesktopLocalizationOptions>()
                ?.Locale;

            var localization = new DesktopLocalizationService(
                new DesktopLocalizationSettingsStore(
                    Path.Combine(temporaryDirectory, "appsettings.Production.json")
                ),
                CultureInfo.GetCultureInfo("en-US"),
                configuredLocale
            );

            Assert.AreEqual(DesktopLocale.SimplifiedChinese, localization.CurrentLocale);
            Assert.AreEqual("欢迎使用 UzonMail", localization.GetText(DesktopTextKey.StartupTitle));
        }
        finally
        {
            DeleteTemporaryDirectory(temporaryDirectory);
        }
    }

    [TestMethod]
    public void Constructor_MapsSystemLanguageAndFallsBackToChinese()
    {
        var englishLocalization = new DesktopLocalizationService(
            new DesktopLocalizationSettingsStore(
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json")
            ),
            CultureInfo.GetCultureInfo("en-GB"),
            configuredLocale: null
        );
        var fallbackLocalization = new DesktopLocalizationService(
            new DesktopLocalizationSettingsStore(
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json")
            ),
            CultureInfo.GetCultureInfo("de-DE"),
            configuredLocale: null
        );

        Assert.AreEqual(DesktopLocale.EnglishUnitedStates, englishLocalization.CurrentLocale);
        Assert.AreEqual(DesktopLocale.SimplifiedChinese, fallbackLocalization.CurrentLocale);
    }

    [TestMethod]
    public void TrySetLocale_PersistsValidLocaleAndNotifiesSubscribers()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var configurationFilePath = Path.Combine(
                temporaryDirectory,
                "appsettings.Production.json"
            );
            File.WriteAllText(
                configurationFilePath,
                "{\"Backend\":{\"WebUrl\":\"http://localhost\"}}"
            );
            var settingsStore = new DesktopLocalizationSettingsStore(configurationFilePath);
            var localization = new DesktopLocalizationService(
                settingsStore,
                CultureInfo.GetCultureInfo("zh-CN"),
                configuredLocale: null
            );
            var changeCount = 0;
            localization.LocaleChanged += (_, _) => changeCount++;

            Assert.IsTrue(localization.TrySetLocale("en-US"));
            Assert.AreEqual(DesktopLocale.EnglishUnitedStates, localization.CurrentLocale);
            using var configuration = JsonDocument.Parse(File.ReadAllText(configurationFilePath));
            Assert.AreEqual(
                "en-US",
                configuration
                    .RootElement.GetProperty(DesktopLocalizationOptions.SectionName)
                    .GetProperty(nameof(DesktopLocalizationOptions.Locale))
                    .GetString()
            );
            Assert.AreEqual(
                "http://localhost",
                configuration.RootElement.GetProperty("Backend").GetProperty("WebUrl").GetString()
            );
            Assert.AreEqual(1, changeCount);
            Assert.AreEqual(
                "Welcome to UzonMail",
                localization.GetText(DesktopTextKey.StartupTitle)
            );
        }
        finally
        {
            DeleteTemporaryDirectory(temporaryDirectory);
        }
    }

    [TestMethod]
    public void TrySetLocale_RejectsUnsupportedLocaleWithoutWritingSettings()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var configurationFilePath = Path.Combine(
                temporaryDirectory,
                "appsettings.Production.json"
            );
            var settingsStore = new DesktopLocalizationSettingsStore(configurationFilePath);
            var localization = new DesktopLocalizationService(
                settingsStore,
                CultureInfo.GetCultureInfo("zh-CN"),
                configuredLocale: null
            );

            Assert.IsFalse(localization.TrySetLocale("de-DE"));
            Assert.IsFalse(File.Exists(configurationFilePath));
            Assert.AreEqual(DesktopLocale.SimplifiedChinese, localization.CurrentLocale);
        }
        finally
        {
            DeleteTemporaryDirectory(temporaryDirectory);
        }
    }

    [TestMethod]
    public void TrySetLocale_RejectsInvalidProductionConfigurationWithoutChangingLocale()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var configurationFilePath = Path.Combine(
                temporaryDirectory,
                "appsettings.Production.json"
            );
            File.WriteAllText(configurationFilePath, "[]");
            var localization = new DesktopLocalizationService(
                new DesktopLocalizationSettingsStore(configurationFilePath),
                CultureInfo.GetCultureInfo("zh-CN"),
                configuredLocale: null
            );
            var changeCount = 0;
            localization.LocaleChanged += (_, _) => changeCount++;

            Assert.IsFalse(localization.TrySetLocale("en-US"));
            Assert.AreEqual(DesktopLocale.SimplifiedChinese, localization.CurrentLocale);
            Assert.AreEqual(0, changeCount);
            Assert.AreEqual("[]", File.ReadAllText(configurationFilePath));
        }
        finally
        {
            DeleteTemporaryDirectory(temporaryDirectory);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "UzonMailDesktopTests",
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(temporaryDirectory);
        return temporaryDirectory;
    }

    private static void DeleteTemporaryDirectory(string temporaryDirectory)
    {
        if (Directory.Exists(temporaryDirectory))
            Directory.Delete(temporaryDirectory, recursive: true);
    }
}
