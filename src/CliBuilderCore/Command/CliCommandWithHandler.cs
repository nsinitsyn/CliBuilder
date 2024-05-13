using CliBuilderCore.Command.Templates;

namespace CliBuilderCore.Command;

internal class CliCommandWithHandler<TCommand> : CliCommand
{
    public CliCommandWithHandler(
        Type commandType, 
        Template inputCommandTemplate, 
        Action<TCommand, TextWriter, CancellationToken> handler,
        string? description)
        : base(commandType, inputCommandTemplate, description)
    {
        Handler = handler;
    }

    public Action<TCommand, TextWriter, CancellationToken> Handler { get; private set; }
}