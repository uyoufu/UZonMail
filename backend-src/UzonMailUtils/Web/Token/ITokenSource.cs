namespace UZonMail.Utils.Web.Token
{
    public interface ITokenSource
    {
        /// <summary>
        /// 用户名
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        string? UserName { get; set; }

        /// <summary>
        /// 是否是超级管理员
        /// </summary>
        bool IsSuperAdmin { get; set; }
    }
}
