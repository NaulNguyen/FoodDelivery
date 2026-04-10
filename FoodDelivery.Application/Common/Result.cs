namespace FoodDelivery.Application.Common;

public class Result
{
    public bool Succeeded { get; protected set; }
    public List<string> Errors { get; protected set; } = new();

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(string error) => new() { Succeeded = false, Errors = new List<string> { error } };
    public static Result Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToList() };
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public new static Result<T> Failure(string error) => new() { Succeeded = false, Errors = new List<string> { error } };
    public new static Result<T> Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToList() };
}
