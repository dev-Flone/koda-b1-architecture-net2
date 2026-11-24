namespace KodaStore.Application.Common;

public enum ResultErrorType
{
    NotFound,
    Conflict
}

public sealed record ResultError(ResultErrorType Type, string Message);

public sealed class Result<T>
{
    public T? Value { get; }
    public ResultError? Error { get; }
    public bool IsSuccess => Error is null;
    private Result(T? value, ResultError? error)
    {
        Value = value;
        Error = error;
    }
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(value, null);
    }
    public static Result<T> Failure(ResultErrorType type, string message)
    {
        return new Result<T>(default, new ResultError(type, message));
    }
}