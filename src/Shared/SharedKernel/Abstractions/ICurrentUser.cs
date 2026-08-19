namespace SharedKernel.Abstractions;

public interface ICurrentUser
{
    Guid? Id { get; }

    string? Name { get; }

    bool IsAuthenticated { get; }
}
