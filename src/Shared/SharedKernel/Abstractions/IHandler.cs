using SharedKernel.Results;

namespace SharedKernel.Abstractions;

/// <summary>Marker for handler discovery. Do not implement directly.</summary>
public interface IHandler;

public interface IQueryHandler<in TQuery, TResponse> : IHandler
{
    Task<Result<TResponse>> ExecuteAsync(TQuery query, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse> : IHandler
{
    Task<Result<TResponse>> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand> : IHandler
{
    Task<Result> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}
