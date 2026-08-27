using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.PagingQuery;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails
{
    /// <summary>
    /// 提供当前用户发件明细的分页查询与只读内容接口。
    /// </summary>
    public class SendingItemController(SqlContext db, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 获取邮件模板数量
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetEmailTemplatesCount(
            long sendingGroupId,
            string filter,
            int itemStatus
        )
        {
            var userId = tokenService.GetUserSqlId();
            // 只能获取自己的发件历史
            var sendingGroup = await db.SendingGroups.FirstOrDefaultAsync(x =>
                x.Id == sendingGroupId && x.UserId == userId
            );
            if (sendingGroup == null)
            {
                return 0.ToSuccessResponse();
            }

            var dbSet = db.SendingItems.Where(x => x.SendingGroupId == sendingGroupId);
            if (itemStatus >= 0)
            {
                var itemStatusEnum = (SendingItemStatus)itemStatus;
                if (itemStatusEnum == SendingItemStatus.Success)
                {
                    dbSet = dbSet.Where(x => x.Status >= SendingItemStatus.Success);
                }
                else if (itemStatusEnum == SendingItemStatus.Failed)
                {
                    dbSet = dbSet.Where(x => x.Status <= SendingItemStatus.Cancel);
                }
                else
                {
                    dbSet = dbSet.Where(x => x.Status == itemStatusEnum);
                }
            }

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    (x.Subject ?? string.Empty).Contains(filter)
                    || (x.RecipientEmails ?? string.Empty).Contains(filter)
                    || (x.SenderEmail ?? string.Empty).Contains(filter)
                );
            }
            var count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮件模板数据
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<SendingItemSummaryDto>>> GetEmailTemplatesData(
            long sendingGroupId,
            string filter,
            Pagination pagination,
            int itemStatus
        )
        {
            var userId = tokenService.GetUserSqlId();
            // 只能获取自己的发件历史
            var sendingGroup = await db.SendingGroups.FirstOrDefaultAsync(x =>
                x.Id == sendingGroupId && x.UserId == userId
            );
            if (sendingGroup == null)
            {
                return new List<SendingItemSummaryDto>().ToSuccessResponse();
            }

            var dbSet = db.SendingItems.Where(x => x.SendingGroupId == sendingGroupId);

            if (itemStatus >= 0)
            {
                var itemStatusEnum = (SendingItemStatus)itemStatus;
                if (itemStatusEnum == SendingItemStatus.Success)
                {
                    dbSet = dbSet.Where(x => x.Status >= SendingItemStatus.Success);
                }
                else if (itemStatusEnum == SendingItemStatus.Failed)
                {
                    dbSet = dbSet.Where(x => x.Status <= SendingItemStatus.Cancel);
                }
                else
                {
                    dbSet = dbSet.Where(x => x.Status == itemStatusEnum);
                }
            }

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    (x.Subject ?? string.Empty).Contains(filter)
                    || (x.RecipientEmails ?? string.Empty).Contains(filter)
                    || (x.SenderEmail ?? string.Empty).Contains(filter)
                );
            }

            var sendingItemRows = await dbSet
                .AsNoTracking()
                .Page(pagination)
                .Select(x => new
                {
                    x.Id,
                    x.Subject,
                    x.SenderEmail,
                    x.Recipients,
                    x.Status,
                    x.SendDate,
                    x.SendResult,
                })
                .ToListAsync();

            var results = sendingItemRows
                .Select(x => new SendingItemSummaryDto
                {
                    Id = x.Id,
                    Subject = x.Subject ?? string.Empty,
                    SenderEmail = x.SenderEmail ?? string.Empty,
                    Recipients = x
                        .Recipients.Select(recipient => new EmailAddressDto
                        {
                            Email = recipient.Email,
                            Name = recipient.Name,
                        })
                        .ToList(),
                    Status = x.Status,
                    SendDate = x.SendDate,
                    SendResult = x.SendResult,
                })
                .ToList();
            return results.ToSuccessResponse();
        }

        [HttpGet("{sendingItemId:long}/body")]
        public async Task<ResponseResult<string?>> GetSendingItemBody(long sendingItemId)
        {
            var userId = tokenService.GetUserSqlId();
            var sendingItem = await db.SendingItems.FirstOrDefaultAsync(x =>
                x.Id == sendingItemId && x.UserId == userId
            );
            if (sendingItem == null)
                return ResponseResult<string?>.Fail("邮件已被删除");
            return sendingItem.Content.ToSuccessResponse();
        }

        /// <summary>
        /// 获取当前用户拥有的已发送邮件完整内容。
        /// </summary>
        /// <param name="sendingItemId">发件项数据库标识。</param>
        /// <param name="cancellationToken">请求取消令牌。</param>
        /// <returns>邮件头、最终正文及附件摘要。</returns>
        [HttpGet("{sendingItemId:long}/detail")]
        public async Task<ResponseResult<SendingItemDetailDto>> GetSendingItemDetail(
            long sendingItemId,
            CancellationToken cancellationToken
        )
        {
            var userId = tokenService.GetUserSqlId();
            var sendingItem = await db
                .SendingItems.AsNoTracking()
                .Include(x => x.Attachments!)
                .ThenInclude(x => x.FileObject)
                .FirstOrDefaultAsync(
                    x => x.Id == sendingItemId && x.UserId == userId,
                    cancellationToken
                );
            if (sendingItem == null)
                return ResponseResult<SendingItemDetailDto>.Fail("邮件不存在或无权访问");

            var sendingItemDetail = new SendingItemDetailDto
            {
                Id = sendingItem.Id,
                Subject = sendingItem.Subject ?? string.Empty,
                SenderEmail = sendingItem.SenderEmail ?? string.Empty,
                SentAt = sendingItem.SendDate,
                Recipients = sendingItem
                    .Recipients.Select(x => new EmailAddressDto { Email = x.Email, Name = x.Name })
                    .ToList(),
                CcRecipients = (sendingItem.CC ?? [])
                    .Select(x => new EmailAddressDto { Email = x.Email, Name = x.Name })
                    .ToList(),
                BccRecipients = (sendingItem.BCC ?? [])
                    .Select(x => new EmailAddressDto { Email = x.Email, Name = x.Name })
                    .ToList(),
                Content = sendingItem.Content ?? string.Empty,
                Attachments = (sendingItem.Attachments ?? [])
                    .Select(x => new SendingItemAttachmentDto
                    {
                        Id = x.Id,
                        DisplayName = x.DisplayName,
                        Size = x.FileObject.Size,
                    })
                    .ToList(),
            };
            return sendingItemDetail.ToSuccessResponse();
        }
    }
}
