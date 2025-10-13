using Newtonsoft.Json.Linq;

namespace UZonMail.DB.SQL.Core.EmailSending
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

            // 计算 inboxes , outboxes, body 的数量
            foreach (var item in excelData)
            {
                if (item is not JObject row) continue;
                var inbox = row.GetValue("inbox")?.ToString();
                var outbox = row.GetValue("outbox")?.ToString();
                var body = row.GetValue("body")?.ToString();

                if (!string.IsNullOrEmpty(inbox))
                {
                    InboxSet.Add(inbox);
                }
                if (!string.IsNullOrEmpty(outbox))
                {
                    OutboxSet.Add(outbox);
                }
                if (!string.IsNullOrEmpty(body))
                {
                    BodyCount++;
                }
            }

            // 解析 status
            InboxStatus = ParseStatus(InboxesCount, TotalCount);
            OutboxStatus = ParseStatus(OutboxesCount, TotalCount);
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

        public int InboxesCount => InboxSet.Count;

        public int OutboxesCount => OutboxSet.Count;

        public int BodyCount { get; }

        public HashSet<string> InboxSet { get; } = [];

        public HashSet<string> OutboxSet { get; } = [];

        public ExcelDataStatus InboxStatus { get; }
        public ExcelDataStatus OutboxStatus { get; }
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
