using CliBuilderCore.Command;
using CliBuilderCore.Command.Templates;
using CliBuilderCore.CommandBuilding;
using CliBuilderCore.Validation;

namespace CliBuilderCore;

public class CliBuilder
{
    private readonly List<CliCommand> _allCommands = new();

    private TextReader _reader = Console.In;
    private TextWriter _writer = Console.Out;
    private TextWriter? _logWriter;
    private bool _supportHelpCommand;
    private bool _generateHelpCommand;

    public CliBuilder ReadFrom(TextReader reader)
    {
        _reader = reader;
        return this;
    }
    
    public CliBuilder WriteTo(TextWriter writer)
    {
        _writer = writer;
        return this;
    }
    
    public CliBuilder RegisterCommand<TCommand>(
        string inputTemplate, 
        Func<ICommandHandler<TCommand>> handlerFactory,
        string? description = null)
        where TCommand : new()
    {
        var commandType = typeof(TCommand);

        var command =
            new CliCommandWithFactory<TCommand>(commandType, new StringTemplate(inputTemplate), handlerFactory, description);
        
        _allCommands.Add(command);

        return this;
    }

    public CliBuilder RegisterCommand<TCommand>(
        string inputTemplate,
        Action<TCommand, TextWriter, CancellationToken> handler,
        string? description = null) 
        where TCommand : new()
    {
        ArgumentNullException.ThrowIfNull(handler);

        var commandType = typeof(TCommand);

        var command =
            new CliCommandWithHandler<TCommand>(commandType, new StringTemplate(inputTemplate), handler, description);
        
        _allCommands.Add(command);

        return this;
    }

    public CliBuilder RegisterCommand<TCommand>(
        TemplateBuilder templateBuilder, 
        Func<ICommandHandler<TCommand>> handlerFactory,
        string? description = null)
        where TCommand : new()
    {
        var commandType = typeof(TCommand);

        var command =
            new CliCommandWithFactory<TCommand>(commandType, templateBuilder.Build(), handlerFactory, description);
        
        _allCommands.Add(command);

        return this;
    }
    
    public CliBuilder RegisterCommand<TCommand>(
        TemplateBuilder templateBuilder,
        Action<TCommand, TextWriter, CancellationToken> handler,
        string? description = null) 
        where TCommand : new()
    {
        ArgumentNullException.ThrowIfNull(handler);

        var commandType = typeof(TCommand);

        var command =
            new CliCommandWithHandler<TCommand>(commandType, templateBuilder.Build(), handler, description);
        
        _allCommands.Add(command);

        return this;
    }

    public CliBuilder LogErrorsTo(TextWriter logWriter)
    {
        _logWriter = logWriter;
        return this;
    }

    public CliBuilder SupportHelpCommand(bool generate = false)
    {
        _supportHelpCommand = true;
        _generateHelpCommand = generate;
        return this;
    }
    
    public Cli Build()
    {
        Validator.Validate(_allCommands, _generateHelpCommand);

        if (_generateHelpCommand)
        {
            var helpOutput = HelpGenerator.Generate(_allCommands);
            RegisterCommand<EmptyCommand>("help", (_, textWriter, _) => textWriter.WriteLine(helpOutput));
        }
        
        return new Cli(_reader, _writer, _allCommands, _logWriter, _supportHelpCommand);
    }
}