using log4net;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.HostedServices;

/// <summary>
/// 恢复进程停止前尚未完成的发送组。
/// </summary>
public sealed class SendCoreRecoveryService(
    SqlContext db,
    ISendingGroupCommandService commandService
) : IScopedServiceAfterStarting
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(SendCoreRecoveryService));

    public int Order => 10_000;

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var groups = await db
            .SendingGroups.AsNoTracking()
            .Where(x => x.Status == SendingGroupStatus.Sending)
            .OrderBy(x => x.Id)
            .ToListAsync(stoppingToken);

        var restoredCount = 0;
        foreach (var group in groups)
        {
            stoppingToken.ThrowIfCancellationRequested();
            try
            {
                await commandService.SendNow(group);
                restoredCount++;
            }
            catch (Exception exception)
            {
                Logger.Error($"恢复发件组 {group.Id} 失败", exception);
            }
        }

        if (restoredCount > 0)
            Logger.Info($"已恢复 {restoredCount} 个发送中的发件组");
    }
}
