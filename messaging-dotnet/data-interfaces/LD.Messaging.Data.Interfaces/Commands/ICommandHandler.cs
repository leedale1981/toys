namespace LD.Messaging.DataInterfaces.Commands;

/// <summary>
/// Generic CQRS command handler interface.
/// Implementations live in the persistence/infrastructure layer.
/// </summary>
/// <typeparam name="TCommand">The command type (must implement <see cref="ICommand"/>).</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>Executes the command.</summary>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

