using UzonMail.Utils.Results;

namespace UzonMail.Utils.Results
{
    public class SuccessResult<T> : Result<T>
    {
        public SuccessResult(T data) : base(true, "success",data)
        {
        }
    }
}
