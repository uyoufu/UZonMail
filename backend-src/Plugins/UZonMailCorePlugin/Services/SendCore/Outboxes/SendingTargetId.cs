namespace UZonMail.Core.Services.SendCore.Outboxes
{
    /// <summary>
    /// 用于在发件箱地址中保存发送项的 Id
    /// </summary>
    public class SendingTargetId(long sendingGroupId)
    {
        public long SendingGroupId { get; private set; } = sendingGroupId;
        public long SendingItemId { get; private set; }

        public SendingTargetId(long sendingGroupId, long sendingItemId) : this(sendingGroupId)
        {
            SendingItemId = sendingItemId;
        }

        public override int GetHashCode()
        {
            return $"{SendingGroupId}_{SendingItemId}".GetHashCode();
        }

        /// <summary>
        /// 是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is SendingTargetId targetId)
            {
                return targetId.SendingGroupId == SendingGroupId && targetId.SendingItemId == SendingItemId;
            }

            return false;
        }
    }
}
