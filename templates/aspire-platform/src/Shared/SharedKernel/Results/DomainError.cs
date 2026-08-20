namespace SharedKernel.Results;

public sealed record DomainError(ErrorType Type, string Code, string Description)
{
    public static DomainError None { get; } =
        new(ErrorType.None, string.Empty, string.Empty);

    public static DomainError NullValue { get; } =
        new(ErrorType.Failure, "Error.NullValue", "A null value was provided.");

    public static DomainError Failure(string code, string description) =>
        new(ErrorType.Failure, code, description);

    public static DomainError Validation(string code, string description) =>
        new(ErrorType.Validation, code, description);

    public static DomainError Problem(string code, string description) =>
        new(ErrorType.Problem, code, description);

    public static DomainError NotFound(string code, string description) =>
        new(ErrorType.NotFound, code, description);

    public static DomainError Conflict(string code, string description) =>
        new(ErrorType.Conflict, code, description);

    public static DomainError Forbidden(string code, string description) =>
        new(ErrorType.Forbidden, code, description);

    public static DomainError Unauthorized(string code, string description) =>
        new(ErrorType.Unauthorized, code, description);
}
