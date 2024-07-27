﻿using UZonMail.DB.SQL.MultiTenant;

namespace UZonMail.Core.Services.UserInfos
{
    /// <summary>
    /// 用户登陆结果
    /// </summary>
    public class UserSignInResult
    {
        public string Token { get; set; }
        public List<string> Access { get; set; }
        public User UserInfo { get; set; }
    }
}