using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UzonMail.Utils.Web.Service;
using UzonMail.Utils.Web.Token;

namespace Uamazing.Utils.Web.Token
{
    /// <summary>
    /// TokenClaim 构建器
    /// </summary>
    public interface ITokenClaimBuilder : IScopedService<ITokenClaimBuilder>
    {
        /// <summary>
        /// 构建 TokenClaim
        /// </summary>
        /// <returns></returns>
        Task<List<Claim>> Build(ITokenSource userInfo);
    }
}
