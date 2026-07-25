namespace UzonMail.CorePlugin.Services.Files
{
    /// <summary>
    /// 文件上传或内容复用的结果。
    /// </summary>
    public sealed record FileUploadResult(long FileUsageId, long CategoryId, bool IsExisting);
}
