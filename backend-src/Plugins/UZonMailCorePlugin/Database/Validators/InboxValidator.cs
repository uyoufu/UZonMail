using FluentValidation;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Emails;

namespace UZonMail.Core.Database.Validators
{
    /// <summary>
    /// 收件箱验证器
    /// </summary>R
    public class InboxValidator : AbstractValidator<Inbox>
    {
        public InboxValidator()
        {
            var pattern = @"^[a-zA-Z0-9_%+-]+(\.[a-zA-Z0-9_%+-]+)*@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";
            // 验证是否为邮箱格式
            RuleFor(x => x.Email).NotEmpty().Must(x => Regex.IsMatch(x, pattern)).WithMessage(x => $"{x.Email} 不是有效的邮箱格式");
        }
    }
}
