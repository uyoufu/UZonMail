using System;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Email;

namespace UZonMail.Core.Services.Plugin
{
    public class EmailDecoratorParams(SendingSetting sendingSetting, SendingItem sendingItem, string outboxEmail) : IEmailDecoratorParams
    {
        public SendingSetting SendingSetting { get; } = sendingSetting;

        public SendingItem SendingItem { get; } = sendingItem;

        /// <summary>
        /// 发件箱
        /// </summary>
        public string OutboxEmail { get; set; } = outboxEmail;
    }
}
