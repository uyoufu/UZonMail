using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Utils.Web.Token
{
    /// <summary>
    /// jwt 授权验证
    /// </summary>
    public static class JWTToken
    {
        /// <summary>
        /// 创建 token
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static string CreateToken(this TokenParams tokenParam, List<Claim> claims)
        {
            // 和 Startup 中的配置一致
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(tokenParam.UniqueSecret));
            JwtSecurityToken token = new(
                issuer: tokenParam.Issuer,
                audience: tokenParam.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: tokenParam.ExpireDate,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }

        /// <summary>
        /// 获取token中的附带的数据
        /// </summary>
        /// <param name="tokenParam"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static JObject GetTokenPayloads(string token)
        {
            // 校验并解析token
            var securityToken = new JwtSecurityTokenHandler().ReadJwtToken(token); //validatedToken:解密后的对象
            var jwtPayload = securityToken.Payload.SerializeToJson(); //获取payload中的数据
            var jobj = JObject.Parse(jwtPayload);
            if (jobj == null) return new JObject();
            return jobj;
        }
    }
}
