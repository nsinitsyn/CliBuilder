namespace ShellBuilderCore.Command;

internal class TextCommandWithHandler<TCommand> : TextCommand
{
    public TextCommandWithHandler(
        Type commandType, 
        string inputCommandTemplate, 
        Action<TCommand, TextWriter, CancellationToken> handler,
        string? description)
        : base(commandType, inputCommandTemplate, description)
    {
        Handler = handler;
    }

    public Action<TCommand, TextWriter, CancellationToken> Handler { get; private set; }
}