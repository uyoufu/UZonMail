using FluentValidation;
using UZonMail.DB.SQL.Core.Emails;

namespace UZonMail.CorePlugin.Database.Validators
{
    /// <summary>
    /// Outbox 验证器
    /// </summary>
    public class OutboxValidator : AbstractValidator<Outbox>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OutboxValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("请输入正确的 Email");
            RuleFor(x => x.Password)
                .Must(
                    (outbox, password) =>
                    {
                        if (Outbox.IsExchangeEmail(outbox.Email))
                        {
                            // 若没有 userName，则不需要密码
                            return !(
                                string.IsNullOrEmpty(outbox.UserName)
                                ^ string.IsNullOrEmpty(outbox.Password)
                            );
                        }

                        return !string.IsNullOrEmpty(password);
                    }
                )
                .WithMessage("请输入邮箱密钥");
            RuleFor(x => x.SmtpHost).NotEmpty().WithMessage("请设置 SmtpHost");
            RuleFor(x => x.SmtpPort)
                .GreaterThan(0)
                .LessThan(65535)
                .WithMessage("SmtpPort 必须在 (0, 65535) 之间");
        }
    }
}
