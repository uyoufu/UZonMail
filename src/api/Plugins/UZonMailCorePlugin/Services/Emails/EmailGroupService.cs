using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Common;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Utils.Database;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;

namespace UzonMail.CorePlugin.Services.Emails
{
    /// <summary>
    /// 邮件组
    /// </summary>
    public class EmailGroupService(SqlContext db, TokenService tokenService)
        : CurdService<EmailGroup>(db)
    {
        /// <summary>
        /// 获取用户的邮箱组
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupType"></param>
        /// <returns></returns>
        public async Task<List<EmailGroup>> GetEmailGroups(
            long userId,
            EmailGroupCategory groupType
        )
        {
            var results = await Db
                .EmailGroups.Where(x => x.UserId == userId && x.Category == groupType)
                .ToListAsync();
            return results;
        }

        /// <summary>
        /// 获取默认的邮箱分组
        /// </summary>
        /// <param name="groupType"></param>
        /// <returns></returns>
        public async Task<EmailGroup> GetDefaultEmailGroup(
            EmailGroupCategory groupType = EmailGroupCategory.Recipient
        )
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            if (tokenPayloads.Count == 0)
                throw new KnownException("无法获取用户信息");

            var defaultGroup = await Db
                .EmailGroups.Where(x =>
                    x.IsDefault && x.UserId == tokenPayloads.UserId && x.Category == groupType
                )
                .FirstOrDefaultAsync();
            if (defaultGroup == null)
            {
                defaultGroup = EmailGroup.GetDefaultEmailGroup(tokenPayloads.UserId, groupType);
                await Db.EmailGroups.AddAsync(defaultGroup);
            }
            await Db.SaveChangesAsync();
            return defaultGroup;
        }

        /// <summary>
        /// 新建邮箱组
        /// 特别注意要修改表中的 type 字段
        /// </summary>
        /// <param name="emailGroup"></param>
        /// <returns></returns>
        public override async Task<EmailGroup> Create(EmailGroup emailGroup)
        {
            // 判断组名是否重复
            if (
                await Db.EmailGroups.AnyAsync(x =>
                    x.UserId == emailGroup.UserId
                    && x.Category == emailGroup.Category
                    && x.Name == emailGroup.Name
                )
            )
            {
                throw new KnownException("组名重复");
            }

            // 新建组
            return await base.Create(emailGroup);
        }

        /// <summary>
        /// 更新组
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public async Task<EmailGroup?> UpdateBoxGroup(
            string name,
            string? description,
            string? icon
        )
        {
            ;
            if (string.IsNullOrEmpty(name))
                throw new KnownException("组名不允许为空");
            // 获取当前用户 id
            var userId = tokenService.GetUserSqlId();
            var emailGroup = new EmailGroup()
            {
                Id = 0,
                Name = name,
                Description = description,
                Icon = icon
            };
            List<string> updatedNames = [name];
            if (!string.IsNullOrEmpty(description))
                updatedNames.Add(description);
            if (!string.IsNullOrEmpty(icon))
                updatedNames.Add(icon);

            var result = await Db.UpdateById(emailGroup, updatedNames);
            await Db.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// 通过 id 删除组
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<bool> DeleteById(long id)
        {
            return Db.RunTransaction(
                async (ctx) =>
                {
                    // 先获取组
                    EmailGroup? group = await ctx
                        .EmailGroups.Where(x => x.Id == id)
                        .Include(x => x.RecipientContacts)
                        .FirstOrDefaultAsync();

                    if (group == null)
                        return true;

                    group.IsDeleted = true;
                    group.RecipientContacts.ForEach(x => x.IsDeleted = true);
                    await ctx.SaveChangesAsync();

                    return true;
                }
            );
        }
    }
}
