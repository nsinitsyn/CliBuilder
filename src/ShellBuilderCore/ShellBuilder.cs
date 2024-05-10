using ShellBuilderCore.Command;
using ShellBuilderCore.Validation;

namespace ShellBuilderCore;

public class ShellBuilder
{
    private readonly List<TextCommand> _allCommands = new();

    private TextReader _reader = Console.In;
    private TextWriter _writer = Console.Out;
    private TextWriter? _logWriter;
    private bool _supportHelpCommand;
    private bool _generateHelpCommand;

    public ShellBuilder ReadFrom(TextReader reader)
    {
        _reader = reader;
        return this;
    }
    
    public ShellBuilder WriteTo(TextWriter writer)
    {
        _writer = writer;
        return this;
    }
    
    public ShellBuilder RegisterCommand<TCommand>(
        string inputTemplate, 
        Func<ICommandHandler<TCommand>> handlerFactory,
        string? description = null)
        where TCommand : new()
    {
        var commandType = typeof(TCommand);

        var command = new TextCommandWithFactory<TCommand>(commandType, inputTemplate, handlerFactory, description);
        _allCommands.Add(command);

        return this;
    }

    public ShellBuilder RegisterCommand<TCommand>(
        string inputTemplate,
        Action<TCommand, TextWriter, CancellationToken> handler,
        string? description = null) 
        where TCommand : new()
    {
        ArgumentNullException.ThrowIfNull(handler);

        var commandType = typeof(TCommand);

        var command = new TextCommandWithHandler<TCommand>(commandType, inputTemplate, handler, description);
        _allCommands.Add(command);

        return this;
    }

    public ShellBuilder LogErrorsTo(TextWriter logWriter)
    {
        _logWriter = logWriter;
        return this;
    }

    public ShellBuilder SupportHelpCommand(bool generate = false)
    {
        _supportHelpCommand = true;
        _generateHelpCommand = generate;
        return this;
    }
    
    public Shell Build()
    {
        Validator.Validate(_allCommands, _generateHelpCommand);

        if (_generateHelpCommand)
        {
            var helpOutput = string.Join(Environment.NewLine,
                _allCommands.Select(x => $"{x.InputCommandTemplate} - {x.Description}"));
            
            RegisterCommand<EmptyCommand>("help", (_, textWriter, _) => textWriter.WriteLine(helpOutput));
        }
        
        return new Shell(_reader, _writer, _allCommands, _logWriter, _supportHelpCommand);
    }
}