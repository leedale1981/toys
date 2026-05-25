namespace LD.Messaging.DataInterfaces.Queries;

/// <summary>
/// Marker interface for CQRS queries.
/// A query represents a read operation that returns data without modifying state.
/// </summary>
/// <typeparam name="TResult">The type of data returned by the query.</typeparam>
public interface IQuery<TResult>
{
}

