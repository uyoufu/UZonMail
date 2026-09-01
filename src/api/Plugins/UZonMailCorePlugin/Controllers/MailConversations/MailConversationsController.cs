using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.CorePlugin.Services.EmailReceiving;
using UzonMail.CorePlugin.Services.MailConversations;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.MailConversations;

/// <summary>
/// 按自有邮箱账号组织的邮件会话。
/// </summary>
public sealed class MailConversationsController(
    TokenService tokenService,
    MailConversationQueryService queryService,
    MailConversationSendService sendService,
    IImapMessageContentService messageContentService
) : ControllerBaseV1
{
    [HttpGet]
    public async Task<ResponseResult<List<MailConversationListItemDto>>> GetConversations(
        long? emailAccountId,
        long? tagId,
        bool unreadOnly,
        string? filter,
        DateTime? beforeAtUtc,
        long? beforeId,
        int limit = 50,
        CancellationToken cancellationToken = default
    ) =>
        (
            await queryService.GetConversationsAsync(
                tokenService.GetUserSqlId(),
                emailAccountId,
                tagId,
                unreadOnly,
                filter,
                beforeAtUtc,
                beforeId,
                limit,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpGet("{conversationId:long}/messages")]
    public async Task<ResponseResult<List<MailConversationMessageDto>>> GetMessages(
        long conversationId,
        DateTime? beforeAtUtc,
        long? beforeId,
        int limit = 50,
        CancellationToken cancellationToken = default
    ) =>
        (
            await queryService.GetMessagesAsync(
                tokenService.GetUserSqlId(),
                conversationId,
                beforeAtUtc,
                beforeId,
                limit,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpGet("messages/{conversationMessageId:long}/content")]
    public async Task<ResponseResult<MailMessageContentDto>> GetContent(
        long conversationMessageId,
        CancellationToken cancellationToken
    ) =>
        (
            await queryService.GetMessageContentAsync(
                tokenService.GetUserSqlId(),
                conversationMessageId,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpGet("messages/{conversationMessageId:long}/attachments/{mimePartId:long}")]
    public async Task<IActionResult> DownloadAttachment(
        long conversationMessageId,
        long mimePartId,
        CancellationToken cancellationToken
    )
    {
        var userId = tokenService.GetUserSqlId();
        var incomingMessageId = await queryService.GetIncomingMessageIdAsync(
            userId,
            conversationMessageId,
            cancellationToken
        );
        var attachment = await messageContentService.GetAttachmentAsync(
            userId,
            incomingMessageId,
            mimePartId,
            cancellationToken
        );
        return File(attachment.Content, attachment.ContentType, attachment.FileName);
    }

    [HttpPost("{conversationId:long}/read")]
    public async Task<ResponseResult<bool>> MarkRead(
        long conversationId,
        CancellationToken cancellationToken
    )
    {
        await queryService.MarkConversationReadAsync(
            tokenService.GetUserSqlId(),
            conversationId,
            cancellationToken
        );
        return true.ToSuccessResponse();
    }

    [HttpPost("{conversationId:long}/messages")]
    public async Task<ResponseResult<SendConversationMessageResult>> Send(
        long conversationId,
        [FromBody] SendConversationMessageRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await sendService.SendAsync(
                tokenService.GetUserSqlId(),
                conversationId,
                request,
                cancellationToken
            )
        ).ToSuccessResponse();
}
