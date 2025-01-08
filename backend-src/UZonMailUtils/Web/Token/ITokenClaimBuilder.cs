using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Organization;
using UZonMail.Utils.Web.Service;

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
        Task<List<Claim>> Build(IServiceProvider serviceProvider, User userInfo);
    }
}
