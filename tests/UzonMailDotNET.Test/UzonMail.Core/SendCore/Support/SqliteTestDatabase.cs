using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

/// <summary>
/// 为 SendCore 测试提供按用例隔离的 SQLite 内存数据库。
/// </summary>
internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private SqliteTestDatabase(SqliteConnection connection, SqlContext db)
    {
        _connection = connection;
        Db = db;
    }

    internal SqlContext Db { get; }

    internal static async Task<SqliteTestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SqlContext>().UseSqlite(connection).Options;
        var db = new SqlContext(options);
        await db.Database.EnsureCreatedAsync();
        return new SqliteTestDatabase(connection, db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
