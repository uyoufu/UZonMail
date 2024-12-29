using FluentValidation;
using UZonMail.DB.SQL.Emails;

namespace UZonMail.Core.Database.Validators
{
    /// <summary>
    /// 收件箱验证器
    /// </summary>
    public class InboxValidator : AbstractValidator<Inbox>
    {
        public InboxValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(x => $"{x.Email} 不是有效的邮箱格式");
        }
    }
}
