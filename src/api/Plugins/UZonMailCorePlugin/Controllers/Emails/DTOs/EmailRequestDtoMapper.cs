using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.DB.SQL.Core.Templates;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 将邮件控制器请求 DTO 映射为现有领域实体。
    /// </summary>
    internal static class EmailRequestDtoMapper
    {
        internal static Outbox ToEntity(this CreateOutboxDto request) =>
            new()
            {
                EmailGroupId = request.EmailGroupId,
                Email = request.Email ?? string.Empty,
                Name = request.Name,
                Description = request.Description,
                Remark = request.Remark,
                Type = request.Type,
                SmtpHost = request.SmtpHost ?? string.Empty,
                SmtpPort = request.SmtpPort,
                UserName = request.UserName,
                Password = request.Password ?? string.Empty,
                ConnectionSecurity = request.ConnectionSecurity,
                ProxyId = request.ProxyId,
                MaxSendCountPerDay = request.MaxSendCountPerDay,
                ReplyToEmails = request.ReplyToEmails,
                Weight = request.Weight,
            };

        internal static Outbox ToEntity(this UpdateOutboxDto request) =>
            new()
            {
                Email = request.Email ?? string.Empty,
                Name = request.Name,
                Type = request.Type,
                SmtpHost = request.SmtpHost ?? string.Empty,
                SmtpPort = request.SmtpPort,
                UserName = request.UserName,
                Password = request.Password ?? string.Empty,
                ConnectionSecurity = request.ConnectionSecurity,
                Description = request.Description,
                ProxyId = request.ProxyId,
                ReplyToEmails = request.ReplyToEmails,
            };

        internal static Inbox ToEntity(this CreateInboxDto request) =>
            new()
            {
                EmailGroupId = request.EmailGroupId,
                Email = request.Email ?? string.Empty,
                Name = request.Name,
                Description = request.Description,
                Remark = request.Remark,
                MinInboxCooldownHours = request.MinInboxCooldownHours,
            };

        internal static Inbox ToEntity(this CreateUngroupedInboxDto request, long emailGroupId) =>
            new()
            {
                EmailGroupId = emailGroupId,
                Email = request.Email ?? string.Empty,
                Name = request.Name,
                Description = request.Description,
                Remark = request.Remark,
                MinInboxCooldownHours = request.MinInboxCooldownHours,
            };

        internal static Inbox ToEntity(this UpdateInboxDto request) =>
            new()
            {
                Email = request.Email ?? string.Empty,
                Name = request.Name,
                Description = request.Description,
                MinInboxCooldownHours = request.MinInboxCooldownHours,
            };

        internal static EmailGroup ToEntity(this CreateEmailGroupDto request) =>
            new()
            {
                Type = request.Type,
                Icon = request.Icon,
                Name = request.Name ?? string.Empty,
                Description = request.Description,
                Order = request.Order,
            };

        internal static EmailGroup ToEntity(this UpdateEmailGroupDto request) =>
            new()
            {
                Name = request.Name ?? string.Empty,
                Description = request.Description,
                Order = request.Order,
            };

        internal static SendingGroup ToEntity(this SendEmailNowDto request) =>
            request.ToEntity(DateTime.MinValue);

        internal static SendingGroup ToEntity(this ScheduleEmailDto request) =>
            request.ToEntity(request.ScheduleDate);

        internal static EmailTemplate ToEntity(this UpsertEmailTemplateDto request) =>
            new()
            {
                Id = request.Id,
                Name = request.Name ?? string.Empty,
                Description = request.Description,
                Content = request.Content ?? string.Empty,
            };

        internal static SmtpInfo ToEntity(this UpdateSmtpInfoDto request) =>
            new()
            {
                Domain = request.Domain ?? string.Empty,
                Host = request.Host ?? string.Empty,
                Port = request.Port,
                ConnectionSecurity = request.ConnectionSecurity,
                EnableSSL = request.EnableSSL,
            };

        private static SendingGroup ToEntity(this SendEmailNowDto request, DateTime scheduleDate) =>
            new()
            {
                Subjects = request.Subjects ?? string.Empty,
                Templates = (request.TemplateIds ?? []).ConvertAll(id => new EmailTemplate
                {
                    Id = id,
                }),
                Body = request.Body,
                Outboxes = (request.OutboxIds ?? []).ConvertAll(id => new Outbox { Id = id }),
                OutboxGroups = (request.OutboxGroupIds ?? []).ConvertAll(id => new IdAndName
                {
                    Id = id,
                }),
                Inboxes = (request.Inboxes ?? []).ConvertAll(ToEmailAddress),
                InboxGroups = (request.InboxGroupIds ?? []).ConvertAll(id => new IdAndName
                {
                    Id = id,
                }),
                CcBoxes = (request.CcBoxes ?? []).ConvertAll(ToEmailAddress),
                BccBoxes = (request.BccBoxes ?? []).ConvertAll(ToEmailAddress),
                Attachments = (request.AttachmentIds ?? []).ConvertAll(id => new FileUsage
                {
                    __fileUsageId = id,
                }),
                Data = request.Data,
                SendBatch = request.SendBatch,
                ProxyIds = request.ProxyIds ?? [],
                ScheduleDate = scheduleDate,
            };

        private static EmailAddress ToEmailAddress(EmailAddressDto address) =>
            new() { Email = address.Email ?? string.Empty, Name = address.Name };
    }
}
