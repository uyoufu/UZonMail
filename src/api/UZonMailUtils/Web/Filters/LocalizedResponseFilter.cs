using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using UzonMail.Utils.Resources.Langs;
using UzonMail.Utils.Web.ResponseModel;
using UzonMail.Utils.Web.Service;

namespace UzonMail.Utils.Web.Filters;

/// <summary>
/// 在 MVC 序列化前将响应中的错误键解析为当前请求语言。
/// </summary>
public sealed class LocalizedResponseFilter(IStringLocalizer<ApiErrorResource> localizer)
    : IAsyncResultFilter, IScopedService
{
    /// <inheritdoc />
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next
    )
    {
        if (context.Result is ObjectResult { Value: ILocalizedResponseResult response }
            && response.LocalizedError is { } localizedError)
        {
            response.Message = localizer[localizedError.Key.ToString(), localizedError.Arguments];
        }

        await next();
    }
}
