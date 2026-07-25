using System.ComponentModel.DataAnnotations;

namespace UzonMail.CorePlugin.Services.Files
{
    /// <summary>
    /// 上传的文件体
    /// </summary>
    public class ObjectFileUploaderBody
    {
        public required string Sha256 { get; set; }
        public DateTime LastModifyDate { get; set; }
        public bool IsPublic { get; set; }

        /// <summary>
        /// 目标文件分类；未指定时使用当前用户的 Default 分类。
        /// </summary>
        public long? CategoryId { get; set; }

        /// <summary>
        /// 文件上传都是使用 file 字段名
        /// 前端传递的名称可能不能 file,可以采用 this.Request.Form.Files; 读取
        /// </summary>
        [Display(Name = "File")]
        public IFormFile? File { get; set; }
    }
}
