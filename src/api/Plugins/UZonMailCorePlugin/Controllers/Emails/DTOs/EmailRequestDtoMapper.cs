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
        internal static EmailGroup ToEntity(this CreateEmailGroupDto request) =>
            new()
            {
                Category = request.Category,
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
                SenderAccounts = (request.SenderAccountIds ?? []).ConvertAll(id => new SenderAccount
                {
                    Id = id
                }),
                SenderAccountGroups = (request.SenderAccountGroupIds ?? []).ConvertAll(
                    id => new IdAndName { Id = id, }
                ),
                Recipients = (request.Recipients ?? []).ConvertAll(RecipientEmailAddress),
                RecipientContactGroups = (request.RecipientContactGroupIds ?? []).ConvertAll(
                    id => new IdAndName { Id = id, }
                ),
                CcBoxes = (request.CcBoxes ?? []).ConvertAll(RecipientEmailAddress),
                BccBoxes = (request.BccBoxes ?? []).ConvertAll(RecipientEmailAddress),
                Attachments = (request.AttachmentIds ?? []).ConvertAll(id => new FileUsage
                {
                    __fileUsageId = id,
                }),
                Data = request.Data,
                SendBatch = request.SendBatch,
                ProxyIds = request.ProxyIds ?? [],
                ScheduleDate = scheduleDate,
            };

        private static EmailAddress RecipientEmailAddress(EmailAddressDto address) =>
            new() { Email = address.Email ?? string.Empty, Name = address.Name };
    }
}
