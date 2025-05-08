using FluentValidation;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Controllers.Settings.Validators
{
    /// <summary>
    /// 组织设置验证器
    /// </summary>
    public class SendingSettingValidator : AbstractValidator<SendingSetting>
    {
        /// <summary>
        /// 初始化组织设置验证器
        /// </summary>
        public SendingSettingValidator()
        {
            RuleFor(x => x.MinOutboxCooldownSecond).GreaterThanOrEqualTo(0).WithMessage("最小发件时间间隔不能小于0");
            RuleFor(x => x.MaxOutboxCooldownSecond).GreaterThanOrEqualTo(0).WithMessage("最大发件时间间隔不能小于0");

            RuleFor(x => x.MaxOutboxCooldownSecond)
                .GreaterThan(x => x.MinOutboxCooldownSecond)
                .WithMessage("最大发件箱冷却时间不能小于最小发件箱冷却时间");
        }
    }
}
