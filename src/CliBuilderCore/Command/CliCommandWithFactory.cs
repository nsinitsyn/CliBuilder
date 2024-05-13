using CliBuilderCore.Command.Templates;

namespace CliBuilderCore.Command;

internal class CliCommandWithFactory<TCommand> : CliCommand
{
    public CliCommandWithFactory(
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