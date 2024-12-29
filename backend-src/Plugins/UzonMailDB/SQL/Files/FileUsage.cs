using System.ComponentModel.DataAnnotations.Schema;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Organization;

namespace UZonMail.DB.SQL.Files
{
    /// <summary>
    /// 文件使用情况
    /// </summary>
    public class FileUsage : SqlId
    {
        /// <summary>
        /// 为了提取前端上传的附件
        /// </summary>
        [NotMapped]
        public long __fileUsageId { get; set; }

        /// <summary>
        /// 拥有者的用户ID
        /// </summary>
        public long OwnerUserId { get; set; }
        public User OwnerUser { get; set; }

        /// <summary>
        /// 唯一名称
        /// 若有唯一名称，则会替换原来的文件名，减少文件存储
        /// </summary>
        public string? UniqueName { get; set; }

        private string _fileName;
        /// <summary>
        /// 文件名（包含后缀）
        /// 若 DisplayName 为空，则 DisplayName = FileName
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                if(string.IsNullOrEmpty(DisplayName))
                {
                    DisplayName = value;
                }
            }
        }

        /// <summary>
        /// 显示名称，给用户展示的名称
        /// 若要根据名称查找文件，则要求唯一
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 文件 id
        /// </summary>
        public long FileObjectId { get; set; }

        /// <summary>
        /// 文件对象
        /// </summary>
        public FileObject FileObject { get; set; }

        /// <summary>
        /// 是否是公共文件
        /// 公共文件不需要权限即可访问
        /// </summary>
        public bool IsPublic { get; set; }
    }
}
