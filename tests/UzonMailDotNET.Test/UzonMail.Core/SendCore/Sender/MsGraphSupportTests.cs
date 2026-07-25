using System.Text;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Newtonsoft.Json;
using UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Sender;

/// <summary>
/// 验证 Graph 参数持久化规则、令牌模型和 MIME 请求编码。
/// </summary>
[TestClass]
public sealed class MsGraphSupportTests
{
    [TestMethod]
    public void ParamsResolver_UsesConfiguredDefaultApplicationWithoutExposingSecret()
    {
        var resolver = CreateResolver();

        resolver.SetGraphInfo(null, new string('r', 90));

        Assert.AreEqual("default-client", resolver.ClientId);
        Assert.AreEqual("default-tenant", resolver.TenantId);
        Assert.AreEqual("default-secret", resolver.ClientSecret);
        Assert.IsTrue(resolver.HasRefreshToken);
        Assert.AreEqual(string.Empty, resolver.GetUserNameForDB());
        Assert.AreEqual(new string('r', 90), resolver.GetPasswordForDB());
    }

    [TestMethod]
    public void ParamsResolver_ParsesCustomApplicationAndPasswordVariants()
    {
        var secretOnly = CreateResolver();
        secretOnly.SetGraphInfo("client/tenant", new string('s', 30));
        var both = CreateResolver();
        both.SetGraphInfo("client", $"{new string('r', 90)}/{new string('s', 30)}");

        Assert.AreEqual("client/tenant", secretOnly.GetUserNameForDB());
        Assert.AreEqual(new string('s', 30), secretOnly.GetPasswordForDB());
        Assert.IsFalse(secretOnly.HasRefreshToken);
        Assert.AreEqual("client", both.GetUserNameForDB());
        Assert.AreEqual($"{new string('r', 90)}/{new string('s', 30)}", both.GetPasswordForDB());
        both.SetRefreshToken(null);
        Assert.AreEqual(new string('s', 30), both.GetPasswordForDB());
    }

    [TestMethod]
    public void ParamsResolver_RejectsMalformedUserNamesAndPasswords()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            CreateResolver().SetGraphInfo("one/two/three")
        );
        Assert.ThrowsExactly<ArgumentException>(() => CreateResolver().SetGraphInfo("client", "short"));
        Assert.ThrowsExactly<ArgumentException>(() =>
            CreateResolver().SetGraphInfo("client", "one/two/three")
        );
    }

    [TestMethod]
    public void AuthenticationResult_DeserializesWireNamesAndCachesExpiry()
    {
        var result = JsonConvert.DeserializeObject<AuthenticationResult2>(
            "{\"access_token\":\"access\",\"token_type\":\"Bearer\",\"scope\":\"mail.send\",\"expires_in\":120,\"refresh_token\":\"refresh\"}"
        )!;

        var firstExpiry = result.ExpireAt;
        var secondExpiry = result.ExpireAt;

        Assert.AreEqual("access", result.AccessToken);
        Assert.AreEqual("Bearer", result.TokenType);
        Assert.AreEqual("mail.send", result.Scope);
        Assert.AreEqual("refresh", result.RefreshToken);
        Assert.AreEqual(firstExpiry, secondExpiry);
        Assert.IsTrue(firstExpiry > DateTime.UtcNow.AddSeconds(100));
        Assert.IsTrue(result.IsPersonalAccount);
    }

    [TestMethod]
    public async Task SendMailRequest_AddsBearerTokenAndBase64EncodesMimeMessage()
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("sender@test.com"));
        message.To.Add(MailboxAddress.Parse("recipient@test.com"));
        message.Subject = "subject";
        message.Body = new TextPart("plain") { Text = "body" };
        using var request = new MsGraphSendMailRequest().WithAccessToken("token").WithMimeMessage(message);

        var encoded = await request.Content!.ReadAsStringAsync();
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

        Assert.AreEqual(HttpMethod.Post, request.Method);
        Assert.AreEqual("Bearer", request.Headers.Authorization!.Scheme);
        Assert.AreEqual("token", request.Headers.Authorization.Parameter);
        Assert.AreEqual("text/plain", request.Content.Headers.ContentType!.MediaType);
        StringAssert.Contains(decoded, "Subject: subject");
        StringAssert.Contains(decoded, "body");
    }

    private static MsGraphParamsResolver CreateResolver()
    {
        var values = new Dictionary<string, string?>
        {
            ["MicrosoftEntraApp:ClientId"] = "default-client",
            ["MicrosoftEntraApp:TenantId"] = "default-tenant",
            ["MicrosoftEntraApp:ClientSecret"] = "default-secret",
        };
        return new MsGraphParamsResolver(
            new ConfigurationBuilder().AddInMemoryCollection(values).Build()
        );
    }
}
