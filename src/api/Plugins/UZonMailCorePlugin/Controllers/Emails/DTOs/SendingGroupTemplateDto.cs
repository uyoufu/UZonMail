using Newtonsoft.Json.Linq;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs;

/// <summary>
/// 从历史发件组恢复新任务时使用的安全只读模型。
/// </summary>
public sealed class SendingGroupTemplateDto
{
    public string ObjectId { get; set; } = string.Empty;
    public string Subjects { get; set; } = string.Empty;
    public List<TemplateReferenceDto> Templates { get; set; } = [];
    public List<SenderAccountReferenceDto> SenderAccounts { get; set; } = [];
    public List<IdNameReferenceDto> SenderAccountGroups { get; set; } = [];
    public List<IdNameReferenceDto> RecipientContactGroups { get; set; } = [];
    public List<EmailAddressDto> Recipients { get; set; } = [];
    public List<EmailAddressDto> CcBoxes { get; set; } = [];
    public List<EmailAddressDto> BccBoxes { get; set; } = [];
    public List<SendingGroupAttachmentDto> Attachments { get; set; } = [];
    public JArray? Data { get; set; }
    public string? Body { get; set; }
    public bool SendBatch { get; set; }
    public List<long> ProxyIds { get; set; } = [];

    public static SendingGroupTemplateDto FromEntity(SendingGroup sendingGroup) =>
        new()
        {
            ObjectId = sendingGroup.ObjectId,
            Subjects = sendingGroup.Subjects,
            Templates = (sendingGroup.Templates ?? []).ConvertAll(
                template => new TemplateReferenceDto(template.Id, template.Name)
            ),
            SenderAccounts = sendingGroup.SenderAccounts.ConvertAll(
                senderAccount => new SenderAccountReferenceDto(
                    senderAccount.Id,
                    senderAccount.EmailAccount.Email,
                    senderAccount.EmailAccount.Name
                )
            ),
            SenderAccountGroups = (sendingGroup.SenderAccountGroups ?? []).ConvertAll(
                group => new IdNameReferenceDto(group.Id, group.Name)
            ),
            RecipientContactGroups = (sendingGroup.RecipientContactGroups ?? []).ConvertAll(
                group => new IdNameReferenceDto(group.Id, group.Name)
            ),
            Recipients = sendingGroup.Recipients.ConvertAll(ToEmailAddressDto),
            CcBoxes = (sendingGroup.CcBoxes ?? []).ConvertAll(ToEmailAddressDto),
            BccBoxes = (sendingGroup.BccBoxes ?? []).ConvertAll(ToEmailAddressDto),
            Attachments = (sendingGroup.Attachments ?? []).ConvertAll(
                attachment => new SendingGroupAttachmentDto(
                    attachment.Id,
                    attachment.FileName,
                    attachment.FileObject.Sha256,
                    attachment.FileObject.Size
                )
            ),
            Data = sendingGroup.Data,
            Body = sendingGroup.Body,
            SendBatch = sendingGroup.SendBatch,
            ProxyIds = sendingGroup.ProxyIds ?? [],
        };

    private static EmailAddressDto ToEmailAddressDto(EmailAddress address) =>
        new() { Email = address.Email, Name = address.Name };
}

public sealed record TemplateReferenceDto(long Id, string Name);

public sealed record SenderAccountReferenceDto(long Id, string Email, string? Name);

public sealed record IdNameReferenceDto(long Id, string Name);

public sealed record SendingGroupAttachmentDto(long Id, string FileName, string Sha256, long Size);
