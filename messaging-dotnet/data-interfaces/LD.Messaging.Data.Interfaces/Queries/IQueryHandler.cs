namespace LD.Messaging.DataInterfaces.Queries;

/// <summary>
/// Generic CQRS query handler interface.
/// Implementations live in the persistence/infrastructure layer.
/// </summary>
/// <typeparam name="TQuery">The query type (must implement <see cref="IQuery{TResult}"/>).</typeparam>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>Executes the query and returns the result.</summary>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

