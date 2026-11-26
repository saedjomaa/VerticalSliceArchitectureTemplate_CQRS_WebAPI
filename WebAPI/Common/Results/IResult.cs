namespace WebAPI.Common.Results;

// Not to be confused with Microsoft.AspNetCore.Http.IResult (Minimal API results).
public interface IResult
{
    bool IsSuccess { get; }
    Error? Error { get; }
}

public interface IResult<TSelf> : IResult where TSelf : IResult<TSelf>
{
    static abstract TSelf Failure(Error error);
}
