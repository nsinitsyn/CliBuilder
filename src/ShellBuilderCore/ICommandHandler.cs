namespace ShellBuilderCore;

public interface ICommandHandler<TCommand>
{
    void Handle(TCommand command, TextWriter writer, CancellationToken cancellationToken);
}