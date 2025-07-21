namespace Helpers.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public int? ErrorCode { get; } // Optional error code for more detailed error handling
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, string? error, int? errorCode = null)
        {
            if (isSuccess && error != null)
            {
                throw new InvalidOperationException("A successful result cannot have an error message.");
            }
            if (!isSuccess && string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("An error message is required for a failed result.", nameof(error));
            }
            IsSuccess = isSuccess;
            Error = error;
            ErrorCode = errorCode;
        }

        public static Result Success() => new Result(true, null);
        public static Result<T> Success<T>(T value) => new Result<T>(true, value, null);
        public static Result Failure(string error) => new Result(false, error);
        public static Result Failure(string error, int code) => new Result(false, error, code);
        public static Result<T> Failure<T>(string error) => new Result<T>(false, default(T), error);
        public static Result<T> Failure<T>(string error, int code) => new Result<T>(false, default(T), error, code);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;
        public T? Value
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException("Cannot access Value on a failed result.");
                }
                return _value;
            }
        }

        protected internal Result(bool isSuccess, T? value, string? error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        protected internal Result(bool isSuccess, T? value, string? error, int errorCode)
            : base(isSuccess, error, errorCode)
        {
            _value = value;
        }
    }
}