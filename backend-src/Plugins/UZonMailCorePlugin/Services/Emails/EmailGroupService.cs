
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Emails;
using UZonMail.Core.Services.Common;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Utils.Database;
using UZonMail.Utils.Web.Exceptions;

namespace UZonMail.Core.Services.Emails
{
    /// <summary>
    /// 邮件组
    /// </summary>
    public class EmailGroupService(SqlContext db, TokenService tokenService) : CurdService<EmailGroup>(db)
    {
        /// <summary>
        /// 获取用户的邮箱组
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupType"></param>
        /// <returns></returns>
        public async Task<List<EmailGroup>> GetEmailGroups(long userId, EmailGroupType groupType)
        {
            var results = await db.EmailGroups.Where(x => x.UserId == userId && x.Type == groupType).ToListAsync();
            return results;
        }

        /// <summary>
        /// 获取默认的邮箱分组
        /// </summary>
        /// <param name="groupType"></param>
        /// <returns></returns>
        public async Task<EmailGroup> GetDefaultEmailGroup(EmailGroupType groupType = EmailGroupType.InBox)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            if (tokenPayloads.Count == 0) throw new KnownException("无法获取用户信息");

            var defaultGroup = await db.EmailGroups.Where(x => x.IsDefault && x.UserId == tokenPayloads.UserId)
                .FirstOrDefaultAsync();
            if (defaultGroup == null)
            {
                defaultGroup = EmailGroup.GetDefaultEmailGroup(tokenPayloads.UserId, groupType);
                await db.EmailGroups.AddAsync(defaultGroup);
            }
            await db.SaveChangesAsync();
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
            if (await db.EmailGroups.AnyAsync(x => x.UserId == emailGroup.UserId && x.Type == emailGroup.Type && x.Name == emailGroup.Name))
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
        public async Task<EmailGroup?> UpdateBoxGroup(string name, string? description, string? icon)
        {
            ; if (string.IsNullOrEmpty(name)) throw new KnownException("组名不允许为空");
            // 获取当前用户 id
            var userId = tokenService.GetUserDataId();
            var emailGroup = new EmailGroup()
            {
                Id = 0,
                Name = name,
                Description = description,
                Icon = icon
            };
            List<string> updatedNames = [name];
            if (!string.IsNullOrEmpty(description)) updatedNames.Add(description);
            if (!string.IsNullOrEmpty(icon)) updatedNames.Add(icon);

            var result = await db.UpdateById(emailGroup, updatedNames);
            await db.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// 通过 id 删除组
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<bool> DeleteById(long id)
        {
            return db.RunTransaction(async (ctx) =>
             {
                 // 先获取组
                 EmailGroup? group = await ctx.EmailGroups.Where(x => x.Id == id)
                 .Include(x => x.Inboxes)
                 .FirstOrDefaultAsync();

                 if (group == null) return true;

                 // 将其它邮件标记为删除
                 List<Inbox> boxes = [];
                 if (group.Inboxes != null)
                 {
                     boxes.AddRange(group.Inboxes);
                 }
                 bool shouldKeepGroup = boxes.Any(x => x.LinkCount > 0);

                 if (shouldKeepGroup)
                 {
                     // 将组标记为删除，同时将组中未使用的邮箱标记为删除
                     group.IsDeleted = true;
                 }
                 else
                 {
                     // 先删除邮箱，再删除组
                     boxes.ForEach(x => ctx.Remove(x));
                     ctx.Remove(group);
                 }
                 await ctx.SaveChangesAsync();

                 return true;
             });
        }
    }
}
