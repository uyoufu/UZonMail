using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailSending
{
    /// <summary>
    /// 邮件地址
    /// </summary>    
    public class EmailAddress : SqlId
    {
        /// <summary>
        /// 邮箱地址
        /// 最后发件时，以便该地址为准
        /// </summary>
        public string Email { get; set; }

        private string? _name;

        /// <summary>
        /// 姓名，若为空则为邮箱地址
        /// </summary>
        public string? Name
        {
            get { return !string.IsNullOrEmpty(_name) ? _name : Email; }
            set { _name = value; }
        }
    }
}
