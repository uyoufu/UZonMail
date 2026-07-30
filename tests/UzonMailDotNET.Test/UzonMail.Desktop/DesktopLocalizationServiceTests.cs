using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UzonMailDesktop.Localization;

namespace UzonMailDotNET.Test.UzonMail.Desktop;

/// <summary>
/// 验证桌面端语言解析、资源切换和本地持久化行为
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
    public void Constructor_UsesPersistedLocaleBeforeSystemLocale()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsStore = new DesktopUserSettingsStore(
                Path.Combine(temporaryDirectory, "desktop-settings.json")
            );
            settingsStore.WriteLocale("zh-CN");

            var localization = new DesktopLocalizationService(
                settingsStore,
                CultureInfo.GetCultureInfo("en-US")
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
            new DesktopUserSettingsStore(
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json")
            ),
            CultureInfo.GetCultureInfo("en-GB")
        );
        var fallbackLocalization = new DesktopLocalizationService(
            new DesktopUserSettingsStore(
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json")
            ),
            CultureInfo.GetCultureInfo("de-DE")
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
            var settingsStore = new DesktopUserSettingsStore(
                Path.Combine(temporaryDirectory, "desktop-settings.json")
            );
            var localization = new DesktopLocalizationService(
                settingsStore,
                CultureInfo.GetCultureInfo("zh-CN")
            );
            var changeCount = 0;
            localization.LocaleChanged += (_, _) => changeCount++;

            Assert.IsTrue(localization.TrySetLocale("en-US"));
            Assert.AreEqual(DesktopLocale.EnglishUnitedStates, localization.CurrentLocale);
            Assert.AreEqual("en-US", settingsStore.ReadLocale());
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
            var settingsStore = new DesktopUserSettingsStore(
                Path.Combine(temporaryDirectory, "desktop-settings.json")
            );
            var localization = new DesktopLocalizationService(
                settingsStore,
                CultureInfo.GetCultureInfo("zh-CN")
            );

            Assert.IsFalse(localization.TrySetLocale("de-DE"));
            Assert.IsNull(settingsStore.ReadLocale());
            Assert.AreEqual(DesktopLocale.SimplifiedChinese, localization.CurrentLocale);
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
