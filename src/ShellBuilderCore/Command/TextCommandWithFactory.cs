using ShellBuilderCore.Command.Templates;

namespace ShellBuilderCore.Command;

internal class TextCommandWithFactory<TCommand> : TextCommand
{
    public TextCommandWithFactory(
        Type commandType, 
        Template inputCommandTemplate,
        Func<ICommandHandler<TCommand>> handlerFactory,
        string? description)
        : base(commandType, inputCommandTemplate, description)
    {
        HandlerFactory = handlerFactory;
    }

    public Func<ICommandHandler<TCommand>> HandlerFactory { get; private set; }
}