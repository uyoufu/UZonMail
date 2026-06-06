using System.Security.Claims;
using Uamazing.Utils.Web.Token;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Organization;
using UzonMail.Utils.Web.Service;
using UzonMail.Utils.Web.Token;

namespace UzonMail.CorePlugin.Utils.Token
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
        public async Task<List<Claim>> Build(ITokenSource userInfo)
        {
            var tokenPayloads = new TokenPayloads(userInfo as User);
            return tokenPayloads;
        }
    }
}
