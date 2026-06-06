using UZonMail.Utils.Results;

namespace UZonMail.Utils.Results
{
    public class ErrorResult<T> : Result<T>
    {
        public ErrorResult(T data, string error) : base(false, error,data)
        {

        }

        public ErrorResult(string error) : base(false, error,default)
        {

        }
    }
}
