using FluentValidation;
using UZonMail.DB.SQL.Organization;

namespace UZonMail.Core.Database.Validators
{
    /// <summary>
    /// 验证用户名称和密码
    /// </summary>
    public class UserIdAndPasswordValidator:AbstractValidator<User>
    {
        public UserIdAndPasswordValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("用户名不能为空");
            RuleFor(x => x.Password).NotEmpty().WithMessage("密码不能为空");
        }
    }
}
