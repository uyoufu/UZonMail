using UZonMail.DB.SQL;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.Services.SendCore.Utils
{
    /// <summary>
    /// 发件项更新器
    /// </summary>
    public class SendingGroupUpdater
    {
        /// <summary>
        /// 更新发送组的发送信息
        /// 更新成功数据与已发送数
        /// </summary>
        /// <returns></returns>
        public static async Task<SendingGroup> UpdateSendingGroupSentInfo(SqlContext sqlContext, long sendingGroupId)
        {
            // 计算成功的数量
            var statusCount = await sqlContext.SendingItems
                .Where(x => x.SendingGroupId == sendingGroupId)
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToListAsync();

            var successCount = statusCount.Where(x => x.Status >= SendingItemStatus.Success).Select(x => x.Count).Sum();
            var sentCount = statusCount.Where(x => x.Status <= SendingItemStatus.Cancel)
            .Select(x => x.Count)
            .Sum() + successCount;

            var sendingGroup = await sqlContext.SendingGroups.FirstAsync(x => x.Id == sendingGroupId);
            sendingGroup.SuccessCount = successCount;
            sendingGroup.SentCount = sentCount;
            await sqlContext.SaveChangesAsync();

            return sendingGroup;
        }
    }
}
