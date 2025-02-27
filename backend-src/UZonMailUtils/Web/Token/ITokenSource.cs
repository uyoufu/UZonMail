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
    }
}
