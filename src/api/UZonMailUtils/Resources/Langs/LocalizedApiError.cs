namespace UzonMail.Utils.Resources.Langs;

/// <summary>
/// 在 HTTP 响应序列化前按当前请求文化解析的错误描述。
/// </summary>
public sealed record LocalizedApiError(ApiErrorKey Key, params object[] Arguments);
