using System.Text.RegularExpressions;
using FluentValidation;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Database.Validators
{
    /// <summary>
    /// 收件箱验证器
    /// </summary>R
    public class RecipientContactValidator : AbstractValidator<RecipientContact>
    {
        public RecipientContactValidator()
        {
            // 验证是否为邮箱格式
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(x => $"{x.Email} 不是有效的邮箱格式");
        }
    }
}
