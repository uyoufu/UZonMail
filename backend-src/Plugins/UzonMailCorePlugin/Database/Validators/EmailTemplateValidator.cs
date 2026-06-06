using FluentValidation;
using UzonMail.DB.SQL.Core.Templates;

namespace UzonMail.CorePlugin.Database.Validators
{
    /// <summary>
    /// 模板验证器
    /// </summary>
    public class EmailTemplateValidator : AbstractValidator<EmailTemplate>
    {
        public EmailTemplateValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(x => $"模板名称不能为空");
            RuleFor(x => x.Content).NotEmpty().WithMessage(x => $"模板内容不能为空");
        }
    }
}
