using System.Security.Claims;
using Uamazing.Utils.Web.Token;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Organization;

namespace UZonMail.Core.Utils.Token
{
    public class TokenClaimsBuilder : ITokenClaimBuilder
    {
        /// <summary>
        /// 生成 TokenClaims
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public async Task<List<Claim>> Build(IServiceProvider serviceProvider, User userInfo)
        {
            var tokenPayloads = new TokenPayloads(userInfo);
            return tokenPayloads;
        }
    }
}
