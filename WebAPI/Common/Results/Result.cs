namespace WebAPI.Common.Results;

public sealed class Result : IResult<Result>
{
    private Result(Error? error) => Error = error;

    public bool IsSuccess => Error is null;
    public Error? Error { get; }

    public static Result Success() => new(null);
    public static Result Failure(Error error) => new(error);
}

public sealed class Result<T> : IResult<Result<T>>
{
    private readonly T? _value;

    private Result(T? value, Error? error)
    {
        _value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;
    public Error? Error { get; }
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Failed results have no value.");

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);
}
