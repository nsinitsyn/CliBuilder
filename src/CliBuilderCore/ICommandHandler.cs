namespace CliBuilderCore;

public interface ICommandHandler<TCommand>
{
    void Handle(TCommand command, TextWriter writer, CancellationToken cancellationToken);
}