using FluentValidation;
using UzonMail.DB.SQL.Core.Permission;

namespace UzonMail.CorePlugin.Database.Validators
{
    public class RoleValidator : AbstractValidator<Role>
    {
        public RoleValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("请输入角色名称");
            RuleFor(x => x.PermissionCodeIds).NotEmpty().WithMessage("请至少选择一个权限");
        }
    }
}
