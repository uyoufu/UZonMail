using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.DB.SQL.Core.Organization;
using UzonMail.Utils.Web.Exceptions;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Files;

/// <summary>
/// 验证文件分类约束及逻辑文件与磁盘内容的一致删除行为。
/// </summary>
[TestClass]
public sealed class FileLifecycleServiceTests
{
    [TestMethod]
    public async Task GetOrCreateDefaultAsync_ReusesImmutableDefaultCategory()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.Users.Add(new User { Id = 11, UserId = "category-owner" });
        await database.Db.SaveChangesAsync();
        var service = new FileCategoryService(database.Db, new FileOperationLockService());

        var first = await service.GetOrCreateDefaultAsync(11);
        var second = await service.GetOrCreateDefaultAsync(11);

        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual(FileCategory.DefaultName, first.Name);
        Assert.IsTrue(first.IsDefault);
        await Assert.ThrowsAsync<KnownException>(() => service.DeleteAsync(11, first.Id));
    }

    [TestMethod]
    public async Task DeleteAsync_UnreferencedFileSoftDeletesMetadataAndDeletesDiskContent()
    {
        var fixture = await CreateFileFixtureAsync(referenceCount: 0);
        await using var database = fixture.Database;
        try
        {
            var service = CreateFileUsageService(database.Db, fixture.RootDirectory);

            var result = await service.DeleteAsync(fixture.UserId, [fixture.FileUsageId]);

            Assert.AreEqual(fixture.FileUsageId, result.DeletedIds.Single());
            Assert.IsFalse(File.Exists(fixture.FilePath));
            var deletedUsage = await database
                .Db.FileUsages.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == fixture.FileUsageId);
            Assert.IsNotNull(deletedUsage);
            Assert.IsTrue(deletedUsage.IsDeleted);
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, true);
        }
    }

    [TestMethod]
    public async Task DeleteAsync_ReferencedFileRejectsWholeOperationAndKeepsDiskContent()
    {
        var fixture = await CreateFileFixtureAsync(referenceCount: 1);
        await using var database = fixture.Database;
        try
        {
            var service = CreateFileUsageService(database.Db, fixture.RootDirectory);

            await Assert.ThrowsAsync<KnownException>(
                () => service.DeleteAsync(fixture.UserId, [fixture.FileUsageId])
            );

            Assert.IsTrue(File.Exists(fixture.FilePath));
            Assert.IsNotNull(await database.Db.FileUsages.FindAsync(fixture.FileUsageId));
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, true);
        }
    }

    [TestMethod]
    public async Task UploadFileObjectAsync_InvalidCategoryRollsBackMetadataAndDiskContent()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootDirectory);
        await using var database = await SqliteTestDatabase.CreateAsync();
        try
        {
            database.Db.Users.Add(new User { Id = 81, UserId = "upload-owner" });
            database.Db.FileBuckets.Add(
                new FileBucket
                {
                    BucketName = "default",
                    RootDir = rootDirectory,
                    IsDefault = true,
                }
            );
            await database.Db.SaveChangesAsync();

            var content = "uploaded-content"u8.ToArray();
            var hash = Convert.ToHexStringLower(SHA256.HashData(content));
            await using var stream = new MemoryStream(content);
            var upload = new FormFile(stream, 0, content.Length, "file", "upload.txt");
            var operationLocks = new FileOperationLockService();
            var categoryService = new FileCategoryService(database.Db, operationLocks);
            var fileStoreService = new FileStoreService(
                database.Db,
                new TestWebHostEnvironment(rootDirectory),
                categoryService,
                operationLocks
            );

            await Assert.ThrowsAsync<KnownException>(
                () =>
                    fileStoreService.UploadFileObjectAsync(
                        81,
                        new ObjectFileUploaderBody
                        {
                            Sha256 = hash,
                            CategoryId = 999,
                            File = upload,
                        }
                    )
            );

            Assert.AreEqual(0, await database.Db.FileObjects.CountAsync());
            var objectDirectory = Path.Combine(rootDirectory, "objects");
            Assert.IsFalse(
                Directory.Exists(objectDirectory)
                    && Directory
                        .EnumerateFiles(objectDirectory, "*", SearchOption.AllDirectories)
                        .Any()
            );
        }
        finally
        {
            Directory.Delete(rootDirectory, true);
        }
    }

    private static FileUsageService CreateFileUsageService(
        global::UzonMail.DB.SQL.SqlContext db,
        string rootDirectory
    )
    {
        var operationLocks = new FileOperationLockService();
        var categoryService = new FileCategoryService(db, operationLocks);
        var fileStoreService = new FileStoreService(
            db,
            new TestWebHostEnvironment(rootDirectory),
            categoryService,
            operationLocks
        );
        return new FileUsageService(db, fileStoreService, categoryService, operationLocks);
    }

    private static async Task<FileFixture> CreateFileFixtureAsync(long referenceCount)
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootDirectory);
        var filePath = Path.Combine(rootDirectory, "objects", "aa", "bb", "content");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "content");

        var database = await SqliteTestDatabase.CreateAsync();
        const long userId = 31;
        var user = new User { Id = userId, UserId = "file-owner" };
        var bucket = new FileBucket
        {
            Id = 41,
            BucketName = "default",
            RootDir = rootDirectory,
            IsDefault = true,
        };
        var category = new FileCategory
        {
            Id = 51,
            OwnerUser = user,
            Name = FileCategory.DefaultName,
            IsDefault = true,
        };
        var fileUsage = new FileUsage
        {
            Id = 61,
            OwnerUser = user,
            Category = category,
            FileName = "content.txt",
            DisplayName = "content.txt",
            DisplayNameKey = "CONTENT.TXT",
            ReferenceCount = referenceCount,
            FileObject = new FileObject
            {
                Id = 71,
                FileBucket = bucket,
                Path = Path.GetRelativePath(rootDirectory, filePath),
                Sha256 = new string('a', 64),
                Size = 7,
                StorageState = FileObjectStorageState.Ready,
            },
        };
        database.Db.FileUsages.Add(fileUsage);
        await database.Db.SaveChangesAsync();
        return new FileFixture(database, rootDirectory, filePath, userId, fileUsage.Id);
    }

    private sealed record FileFixture(
        SqliteTestDatabase Database,
        string RootDirectory,
        string FilePath,
        long UserId,
        long FileUsageId
    );

    private sealed class TestWebHostEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "UzonMail.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
