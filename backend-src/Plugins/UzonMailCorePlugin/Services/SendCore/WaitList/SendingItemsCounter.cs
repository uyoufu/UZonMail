namespace UZonMail.CorePlugin.Services.SendCore.WaitList
{
    public class SendingItemsCounter
    {
        private int _successCount;
        public int SuccessCount => _successCount;

        private int _sentCount;
        public int SentCount => _sentCount;

        private int _totalCount;
        public int TotalCount => _totalCount;

        /// <summary>
        /// 添加发送的数量
        /// </summary>
        /// <param name="count"></param>
        public void IncreaseTotalCount(int count = 1)
        {
            Interlocked.Add(ref _totalCount, count);
        }

        /// <summary>
        /// 增加发送完成的数量
        /// </summary>
        /// <param name="success"></param>
        public void IncreaseSentCount(bool success)
        {
            Interlocked.Increment(ref _sentCount);
            if (success)
                Interlocked.Increment(ref _successCount);
        }
    }
}
