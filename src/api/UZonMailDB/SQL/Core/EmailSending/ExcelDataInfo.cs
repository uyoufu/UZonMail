using Newtonsoft.Json.Linq;

namespace UzonMail.DB.SQL.Core.EmailSending
{
    public class ExcelDataInfo
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 传入的 excelData 要进行非空检查
        /// </summary>
        /// <param name="excelData"></param>
        public ExcelDataInfo(JArray excelData)
        {
            TotalCount = excelData.Count;
            var recipientRowCount = 0;

            // 计算 recipientContacts , senderAccounts, body 的数量
            foreach (var item in excelData)
            {
                if (item is not JObject row)
                    continue;
                var recipientEmail = row.GetValue("recipientEmail")?.ToString()?.Trim();
                var senderEmail = row.GetValue("senderEmail")?.ToString();
                var body = row.GetValue("body")?.ToString();

                if (!string.IsNullOrWhiteSpace(recipientEmail))
                {
                    recipientRowCount++;
                    RecipientEmails.Add(recipientEmail);
                }
                if (!string.IsNullOrEmpty(senderEmail))
                {
                    SenderEmails.Add(senderEmail);
                }
                if (!string.IsNullOrEmpty(body))
                {
                    BodyCount++;
                }
            }

            // 解析 status
            RecipientValidationStatus = ParseStatus(recipientRowCount, TotalCount);
            SenderAccountStatus = ParseStatus(SenderAccountCount, TotalCount);
            BodyStatus = ParseStatus(BodyCount, TotalCount);
        }

        private ExcelDataStatus ParseStatus(int count, int totalCount)
        {
            if (count == 0)
            {
                return ExcelDataStatus.Empty;
            }
            if (count >= totalCount)
            {
                return ExcelDataStatus.All;
            }
            return ExcelDataStatus.Some;
        }

        public int RecipientCount => RecipientEmails.Count;

        public int SenderAccountCount => SenderEmails.Count;

        public int BodyCount { get; }

        public HashSet<string> RecipientEmails { get; } = [];

        public HashSet<string> SenderEmails { get; } = [];

        public ExcelDataStatus RecipientValidationStatus { get; }
        public ExcelDataStatus SenderAccountStatus { get; }
        public ExcelDataStatus BodyStatus { get; }
    }

    public enum ExcelDataStatus
    {
        /// <summary>
        /// 为空
        /// </summary>
        Empty,

        /// <summary>
        /// 部分
        /// </summary>
        Some,

        /// <summary>
        /// 全部
        /// </summary>
        All,
    }
}
