using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Validators;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails;

/// <summary>
/// IMAP 服务器参数推断接口。
/// </summary>
public sealed class ImapInfoController(ImapInfoService imapInfoService) : ControllerBaseV1
{
    [HttpGet("guess")]
    public async Task<ResponseResult<ImapInfo>> GuessImapInfo(string email)
    {
        if (!email.IsValidEmail())
            return ResponseResult<ImapInfo>.Fail("邮箱格式不正确");

        var results = await imapInfoService.GuessImapInfos([email]);
        return results[email].ToSuccessResponse();
    }

    [HttpPost("guess")]
    public async Task<ResponseResult<List<ImapInfo>>> GuessImapInfo([FromBody] List<string> emails)
    {
        var results = await imapInfoService.GuessImapInfos(emails);
        return results.Values.DistinctBy(info => info.Host).ToList().ToSuccessResponse();
    }
}
