using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;

namespace UZonMail.CorePlugin.Database.Upgrade.Updaters
{
    public class UpdateOutboxConectionSecurity(SqlContext db) : IDatabaseUpdater
    {
        public Version Version => new("0.14.0.0");

        public async Task ExecuteAsync()
        {
            await db.Outboxes.UpdateAsync(
                x => x.ConnectionSecurity == ConnectionSecurity.None,
                x => x.SetProperty(y => y.ConnectionSecurity, ConnectionSecurity.SSL)
            );
        }
    }
}
