namespace UZonMail.DB.Managers.Cache
{
    public class DBCacheKey(Type type, string argString)
    {
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type => type;

        public string Arg => argString;

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Arg);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not DBCacheKey other)
                return false;
            return other.Type == Type && Arg!.Equals(other.Arg);
        }

        public override string ToString()
        {
            return $"{Type.FullName}:{Arg}";
        }
    }
}
