using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UZonMail.DB.SQL.Emails;

namespace UZonMail.DB.SQL.EntityConfigs
{
    /// <summary>
    /// 配置实体类型
    /// </summary>
    public interface IEntityTypeConfig
    {
        /// <summary>
        /// 配置实体
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(ModelBuilder builder);
    }
}
