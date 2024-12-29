using System.ComponentModel.DataAnnotations;

namespace UZonMail.Core.Services.Files
{
    /// <summary>
    /// 上传的文件体
    /// </summary>
    public class ObjectFileUploaderBody
    {
        public string Sha256 { get; set; }
        public DateTime LastModifyDate { get; set; }
        public bool IsPublic { get; set; }
        /// <summary>
        /// 文件上传都是使用 file 字段名
        /// 前端传递的名称可能不能 file,可以采用 this.Request.Form.Files; 读取
        /// </summary>
        [Display(Name = "File")]
        public IFormFile File { get; set; }

        /// <summary>
        /// 唯一名称
        /// </summary>
        public string? UniqueName { get; set; }
    }
}
