using Newtonsoft.Json.Linq;
using System.Security.Claims;
using UZonMail.DB.SQL.Organization;
using UZonMail.DB.SQL.Permission;

namespace UZonMail.Core.Services.Settings
{
    /// <summary>
    /// 生成 token 时的 payload
    /// </summary>
    public class TokenPayloads : List<Claim>
    {
        public TokenPayloads(User? userInfo)
        {
            if (userInfo == null)
                return;

            Add(new Claim("userId", userInfo.Id.ToString()));
            Add(new Claim("userName", userInfo.UserId));
            // 授予角色，接口权限控制
            Add(new Claim(ClaimTypes.Role, userInfo.IsSuperAdmin ? UserRoleNames.Admin.ToString() : UserRoleNames.User.ToString()));
            // signalR 需要使用此处的 Name
            Add(new Claim(ClaimTypes.Name, userInfo.Id.ToString()));
            // 参考:https://learn.microsoft.com/zh-cn/aspnet/core/signalr/groups?view=aspnetcore-8.0
            Add(new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()));
            Add(new Claim("organizationId", userInfo.OrganizationId.ToString()));
            Add(new Claim("departmentId", userInfo.DepartmentId.ToString()));
        }

        public TokenPayloads(JObject? tokenObj)
        {
            if (tokenObj == null)
                return;

            foreach (var item in tokenObj)
            {
                Add(new Claim(item.Key, item.Value?.ToString() ?? ""));
            }
        }

        /// <summary>
        /// 使用 key 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] => this.FirstOrDefault(x => x.Type == key)?.Value ?? "";

        public long UserId => long.Parse(this["userId"]);
        public long OrganizationId => long.Parse(this["organizationId"]);
        public long DepartmentId => long.Parse(this["departmentId"]);
        public string UserName => this["userName"];
    }
}
