using log4net;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using UZonMail.Core.Database.SQL.EmailSending;
using UZonMail.Core.Services.Emails;
using UZonMail.Core.Services.Plugin;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.Services.SendCore.WaitList
{
    /// <summary>
    /// SendItem的元数据
    /// </summary>
    public class SendItemMeta
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SendItemMeta));

        public SendItemMeta(long sendingItemId)
        {
            SendingItemId = sendingItemId;
        }

        public SendItemMeta(long sendingItemId, long outboxId)
        {
            SendingItemId = sendingItemId;
            OutboxId = outboxId;
        }

        /// <summary>
        /// 发件项
        /// </summary>
        public long SendingItemId { get; private set; }

        /// <summary>
        /// 发件箱 Id，这个是系统初始值，过程中不会修改
        /// 程序通过这个判断是否属于特定发件箱
        /// </summary>
        public long OutboxId { get; set; }

        /// <summary>
        /// 尝试次数
        /// </summary>
        private int _triedCount = 0;
        public int TriedCount => _triedCount;

        /// <summary>
        /// 是否被删除
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        #region 容器
        public SendingItemMetaList Parent { get; private set; }
        public void SetParent(SendingItemMetaList metaList)
        {
            Parent = metaList;
        }

        /// <summary>
        /// 完成：成功、失败、重试都调用该接口
        /// 成功，失败：清除回收站数据
        /// 其它状态：重试
        /// </summary>
        /// <param name="success"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void Done()
        {
            if (Parent == null) throw new NullReferenceException("未设置父容器");

            if (Status.HasFlag(SendItemMetaStatus.Success))
            {
                Parent.ClearRecycleBin(SendingItemId, true);
            }
            else if (Status.HasFlag(SendItemMetaStatus.Error))
            {
                Parent.ClearRecycleBin(SendingItemId, false);
            }
            else
            {
                Retry();
            }
        }

        /// <summary>
        /// 重试
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        private void Retry()
        {
            _triedCount++;
            if (Parent == null) throw new NullReferenceException("未设置父容器");
            Parent.MoveRecycleToWaitList(SendingItemId);
        }
        #endregion

        #region 状态
        /// <summary>
        /// 状态
        /// </summary>
        public SendItemMetaStatus Status { get; private set; }

        /// <summary>
        /// 状态对应的消息体
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void SetStatus(SendItemMetaStatus status, string message = "")
        {
            Status = status;
            Message = message;
        }

        /// <summary>
        /// 状态为失败或者成功
        /// </summary>
        /// <returns></returns>
        public bool IsErrorOrSuccess()
        {
            return Status.HasFlag(SendItemMetaStatus.Error) || Status.HasFlag(SendItemMetaStatus.Success);
        }
        #endregion

        #region 重写相等
        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is SendItemMeta other && SendingItemId == other.SendingItemId;
        }

        public override int GetHashCode()
        {
            return SendingItemId.GetHashCode();
        }
        #endregion

        #region 运行时到一定阶段时,动态添加的数据
        /// <summary>
        /// 是否已经初始化了
        /// </summary>
        public bool Initialized { get; set; } = false;

        public SendingItem SendingItem { get; private set; }
        public void SetSendingItem(SendingItem sendingItem)
        {
            SendingItem = sendingItem;
            // 初始化其它项
            BodyData = new SendingItemExcelData(sendingItem.Data);
        }

        /// <summary>
        /// 正文变量数据
        /// </summary>
        public SendingItemExcelData? BodyData { get; private set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<FileInfo> Attachments { get; } = [];
        /// <summary>
        /// 解析附件数据
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        public async Task ResolveAttachments(SendingContext sendingContext)
        {
            if (Attachments.Count > 0) return;

            var fileUsageIds = SendingItem.Attachments?.Select(x => x.Id).ToList() ?? [];
            if (fileUsageIds.Count == 0) return;

            // 查找文件
            var attachments = await sendingContext.SqlContext.FileUsages.Where(f => fileUsageIds.Contains(f.Id))
               .Include(x => x.FileObject)
               .ThenInclude(x => x.FileBucket)
               .Select(x => new { fullPath = $"{x.FileObject.FileBucket.RootDir}/{x.FileObject.Path}", fileName = x.DisplayName ?? x.FileName })
               .ToListAsync();

            Attachments.AddRange(attachments.Select(x => new FileInfo(x.fullPath)).Where(x => x.Exists));
        }


        #region HTML 正文原始内容
        private string _bodyOrigin;
        public string HtmlBody { get; private set; } = string.Empty;
        public async Task SetHtmlBody(SendingContext sendingContext, string htmlBody)
        {
            _bodyOrigin = htmlBody;

            // 替换变量
            HtmlBody = ReplaceVariables(_bodyOrigin);
            // 调用修饰器添加额外的值
            HtmlBody = await StartDecorators(sendingContext, HtmlBody);
        }

        /// <summary>
        /// 替换变量
        /// </summary>
        /// <param name="originText"></param>
        /// <returns></returns>
        private string ReplaceVariables(string originText)
        {
            if (string.IsNullOrEmpty(originText)) return originText;
            // 替换正文变量
            if (BodyData == null) return originText;

            foreach (var item in BodyData)
            {
                if (item.Value == null) continue;
                // 使用正则进行替换
                var regex = new Regex(@"\{\{\s*" + item.Key + @"\s*\}\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                originText = regex.Replace(originText, item.Value.ToString());
            }
            return originText;
        }

        /// <summary>
        /// 调用正文装饰器
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="htmlBody"></param>
        /// <returns></returns>
        private async Task<string> StartDecorators(SendingContext sendingContext, string htmlBody)
        {
            var decoratorParams = await GetEmailDecoratorParams(sendingContext);
            var decorateService = sendingContext.Provider.GetRequiredService<EmailBodyDecorateService>();
            return await decorateService.Decorate(decoratorParams, htmlBody);
        }

        /// <summary>
        /// 获取装饰器参数
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        public async Task<EmailDecoratorParams> GetEmailDecoratorParams(SendingContext sendingContext)
        {
            var outbox = sendingContext.OutboxAddress;

            var orgSetting = await sendingContext.Provider.GetRequiredService<AppSettingsManager>()
                .GetSetting<SendingSetting>(sendingContext.SqlContext, outbox.UserId);

            return new EmailDecoratorParams(orgSetting, SendingItem, outbox.Email);
        }
        #endregion

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; private set; } = string.Empty;
        public void SetSubject(string subject)
        {
            Subject = subject;
        }

        /// <summary>
        /// 最大重试次数
        /// 默认不重试，0
        /// </summary>
        public int MaxRetryCount { get; private set; }
        public async Task SetMaxRetryCount(int maxRetryCount)
        {
            MaxRetryCount = maxRetryCount;
        }

        /// <summary>
        /// 可用的代理，包括由数据或者发件箱指定的代理
        /// </summary>
        public List<long> AvailableProxyIds { get; set; } = [];

        /// <summary>
        /// 代理的 Id
        /// </summary>
        public long ProxyId
        {
            get
            {
                if (Outbox == null && SendingItem == null) return 0;

                // sendingItem 中的 proxyId 优先于 outbox 中的 proxyId
                long proxyId = 0;
                if (Outbox != null && Outbox.ProxyId > 0)
                {
                    proxyId = Outbox.ProxyId;
                }
                if (SendingItem != null && SendingItem.ProxyId > 0)
                {
                    proxyId = SendingItem.ProxyId;
                }

                return proxyId;
            }
        }

        /// <summary>
        /// 发件箱
        /// 这个值是动态赋予的
        /// </summary>
        public OutboxEmailAddress Outbox { get; private set; }
        public void SetOutbox(OutboxEmailAddress outbox)
        {
            Outbox = outbox;
        }

        /// <summary>
        /// 回信人
        /// </summary>
        public List<string> ReplyToEmails { get; private set; } = [];
        public void SetReplyToEmails(List<string> replyToEmails, List<string> globalReplyToEmails)
        {
            ReplyToEmails = replyToEmails;
            if (replyToEmails != null && replyToEmails.Count > 0) return;

            // 使用全局回复               
            ReplyToEmails = globalReplyToEmails;

        }
        #endregion

        #region 发件中需要用到的方法与属性
        /// <summary>
        /// 验证数据
        /// </summary>
        /// <param name="status">等于1，表示正常，2 表示发件箱有问题，3 表示收件箱有问题，4 表示内容有问题</param>
        /// <returns></returns>
        public bool Validate(out int status)
        {
            status = 1;
            // 验证发件箱
            if (Outbox == null || string.IsNullOrEmpty(Outbox.Email))
            {
                _logger.Warn($"数据验证失败: 发件项 {SendingItemId} 发件箱不存在");
                status = 2;
                return false;
            }

            // 验证收件箱
            if (SendingItem == null || SendingItem.Inboxes.Count == 0)
            {
                _logger.Warn($"数据验证失败: 发件项 {SendingItemId} 收件箱为空");
                status = 3;
                return false;
            }

            // 验证内容
            if (string.IsNullOrEmpty(HtmlBody))
            {
                _logger.Warn($"数据验证失败: 发件项 {SendingItemId} 正文为空");
                status = 4;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public long UserId => SendingItem.UserId;

        /// <summary>
        /// 收件箱
        /// </summary>
        public List<EmailAddress> Inboxes => SendingItem.Inboxes;

        /// <summary>
        /// 抄送人
        /// </summary>
        public List<EmailAddress>? CC => SendingItem.CC;

        /// <summary>
        /// 密送
        /// </summary>
        public List<EmailAddress>? BCC => SendingItem.BCC;
        #endregion
    }
}
