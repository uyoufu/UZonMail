using UZonMail.Utils.Results;

namespace UZonMail.Utils.Results
{
    public class SuccessResult<T> : Result<T>
    {
        public SuccessResult(T data) : base(true, "success",data)
        {
        }
    }
}
