using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Database.Upgrade.Updaters
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
