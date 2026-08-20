using SharedKernel.Results;

namespace SharedKernel.Exceptions;

public sealed class DomainException(DomainError error)
    : Exception(error?.Description ?? string.Empty)
{
    public DomainError Error { get; } = error ?? DomainError.NullValue;
}
