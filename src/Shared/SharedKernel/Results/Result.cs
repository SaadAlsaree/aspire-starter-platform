namespace SharedKernel.Results;

public class Result
{
    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess != (error == DomainError.None))
        {
            throw new ArgumentException(
                "A result must carry an error when it fails, and none when it succeeds.",
                nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError Error { get; }

    public static Result Success() => new(true, DomainError.None);

    public static Result<TValue> Success<TValue>(TValue value) => new(value);

    public static Result Failure(DomainError error) => new(false, error);

    public static Result<TValue> Failure<TValue>(DomainError error) => new(error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? value;

    internal Result(TValue value)
        : base(true, DomainError.None) => this.value = value;

    internal Result(DomainError error)
        : base(false, error)
    {
    }

    public TValue Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("A failed result has no value.");

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(DomainError error) => new(error);
}
