using FluentValidation;
using UZonMail.DB.SQL.Core.Permission;

namespace UZonMail.Core.Database.Validators
{
    public class UserRoleValidator : AbstractValidator<UserRoles>
    {
        public UserRoleValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("请指定用户");
            RuleFor(x=>x.Roles).NotEmpty().WithMessage("请至少选择一个角色");
        }
    }
}
