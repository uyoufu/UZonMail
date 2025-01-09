using System.Security.Claims;
using Uamazing.Utils.Web.Token;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Organization;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Utils.Token
{
    /// <summary>
    /// TokenClaims 生成器
    /// </summary>
    public class TokenClaimsBuilder : ITokenClaimBuilder
    {
        /// <summary>
        /// 生成 TokenClaims
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public async Task<List<Claim>> Build(User userInfo)
        {
            var tokenPayloads = new TokenPayloads(userInfo);
            return tokenPayloads;
        }
    }
}
