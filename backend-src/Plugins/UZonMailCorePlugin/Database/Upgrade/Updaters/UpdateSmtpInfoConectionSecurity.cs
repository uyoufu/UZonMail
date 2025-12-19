using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;

namespace UZonMail.CorePlugin.Database.Upgrade.Updaters
{
    public class UpdateSmtpInfoConectionSecurity(SqlContext db) : IDatabaseUpdater
    {
        public Version Version => new("0.15.2.0");

        public async Task ExecuteAsync()
        {
            // 使用 sql 语句更新
            // ConnectionSecurity 为 2 改为 1，4 改为 2，8 改为 3
            var oldValue = 2;
            var newValue = 1;
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE SmtpInfos SET ConnectionSecurity = {newValue} WHERE ConnectionSecurity = {oldValue}"
            );

            oldValue = 4;
            newValue = 2;
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE SmtpInfos SET ConnectionSecurity = {newValue} WHERE ConnectionSecurity = {oldValue}"
            );

            oldValue = 8;
            newValue = 3;
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE SmtpInfos SET ConnectionSecurity = {newValue} WHERE ConnectionSecurity = {oldValue}"
            );
        }
    }
}
