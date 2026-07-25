using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.DB.SQL.Core.Organization;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

/// <summary>
/// 验证数据库附件引用到本地有效文件的解析规则。
/// </summary>
[TestClass]
public sealed class SendAttachmentResolverTests
{
    [TestMethod]
    public async Task ResolveAsync_WithoutReferencesReturnsEmptyResult()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var results = await new SendAttachmentResolver(database.Db).ResolveAsync(
            new SendingItem { Attachments = null }
        );

        Assert.IsEmpty(results);
    }

    [TestMethod]
    public async Task ResolveAsync_ReturnsExistingFilesAndSkipsMissingFiles()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootDirectory);
        var existingPath = Path.Combine(rootDirectory, "existing.txt");
        await File.WriteAllTextAsync(existingPath, "attachment");
        try
        {
            await using var database = await SqliteTestDatabase.CreateAsync();
            var owner = new User { Id = 1, UserId = "owner" };
            var bucket = new FileBucket { Id = 1, RootDir = rootDirectory };
            var existing = CreateUsage(1, "existing.txt", "display.txt", owner, bucket);
            var missing = CreateUsage(2, "missing.txt", string.Empty, owner, bucket);
            database.Db.FileUsages.AddRange(existing, missing);
            await database.Db.SaveChangesAsync();

            var results = await new SendAttachmentResolver(database.Db).ResolveAsync(
                new SendingItem
                {
                    Attachments = [new FileUsage { Id = existing.Id }, new FileUsage { Id = missing.Id }],
                }
            );

            Assert.HasCount(1, results);
            Assert.AreEqual("display.txt", results[0].FileName);
            Assert.AreEqual(existingPath, results[0].File.FullName);
        }
        finally
        {
            Directory.Delete(rootDirectory, true);
        }
    }

    private static FileUsage CreateUsage(
        long id,
        string path,
        string displayName,
        User owner,
        FileBucket bucket
    ) =>
        new()
        {
            Id = id,
            OwnerUser = owner,
            FileName = path,
            DisplayName = displayName,
            FileObject = new FileObject
            {
                Id = id,
                Path = path,
                FileBucket = bucket,
            },
        };
}
