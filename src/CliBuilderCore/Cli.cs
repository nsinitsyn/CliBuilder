using System.Globalization;
using System.Reflection;
using CliBuilderCore.Command;
using CliBuilderCore.Command.Templates;
using CliBuilderCore.Parsing;

namespace CliBuilderCore;

public class Cli
{
    private readonly TextReader _reader;
    private readonly TextWriter _writer;
    private readonly List<CliCommand> _allCommands = new();
    private readonly TextWriter? _logWriter;
    private readonly bool _supportHelpCommand;
    
    internal Cli(
        TextReader reader,
        TextWriter writer,
        List<CliCommand> allCommands,
        TextWriter? logWriter,
        bool supportHelpCommand)
    {
        _reader = reader;
        _writer = writer;
        _allCommands = allCommands;
        _logWriter = logWriter;
        _supportHelpCommand = supportHelpCommand;
    }
    
    public void Run(CancellationToken cancellationToken)
    {
        string? unparsedCommand = null;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                unparsedCommand = _reader.ReadLine();

                if (string.IsNullOrEmpty(unparsedCommand?.Trim()))
                {
                    continue;
                }

                bool foundMatch = false;
                bool parsingError = false;
                CliCommand? command = null;
                object? commandInstance = null;
                foreach (var cmd in _allCommands)
                {
                    if (cmd.Template is StringTemplate stringTemplate)
                    {
                        if (!StringTemplateParser.TryParse(unparsedCommand.Trim(), stringTemplate.InputString,
                                out var items))
                        {
                            continue;
                        }
                        
                        ArgumentNullException.ThrowIfNull(items);

                        foundMatch = true;
                        command = cmd;
                        commandInstance = CreateAndFillCommandInstance(cmd, items);

                        break;
                    }

                    if (cmd.Template is ParameterizedTemplate parameterizedTemplate)
                    {
                        var parsingResult =
                            ParameterizedTemplateParser.TryParse(unparsedCommand.Trim(), parameterizedTemplate, cmd);

                        if (parsingResult is { Parsed: false, CommandMatchedByStartedKeywords: false })
                        {
                            continue;
                        }

                        // Command was found but there is parameters parsing error.
                        foundMatch = true;

                        parsingError = ProcessParsingResult(parsingResult, parameterizedTemplate.Name);
                        if (parsingError)
                        {
                            break;
                        }

                        if (parsingResult.Parsed)
                        {
                            command = cmd;
                            commandInstance = parsingResult.CommandInstance;
                        }
                        
                        break;
                    }
                }

                if (!foundMatch)
                {
                    var additional = _supportHelpCommand ? " Type 'help' for getting available commands." : string.Empty;
                    _writer.WriteLine($"Command not found.{additional}");

                    continue;
                }

                if (parsingError)
                {
                    continue;
                }
                
                FindAndInvokeHandler(command!, commandInstance!, unparsedCommand, cancellationToken);
            }
            catch (FormatException ex)
            {
                _logWriter?.WriteLine(
                    $"Format exception during command parsing: {unparsedCommand}. Exception: {ex}");
                
                _writer.WriteLine("Incorrect command format.");
            }
            catch (Exception ex)
            {
                _logWriter?.WriteLine(
                    $"Unexpected exception during command parsing: {unparsedCommand}. Exception: {ex}");
                
                _writer.WriteLine("Error during command processing.");
            }
        }
    }

    private bool ProcessParsingResult(ParsingResult parsingResult, string commandName)
    {
        if (parsingResult is { Parsed: false, NotFoundParameterName: not null })
        {
            _writer.WriteLine(
                $"Parameter {parsingResult.NotFoundParameterName} is unknown for command {commandName}.");

            return true;
        }
                        
        if (parsingResult is { Parsed: false, DuplicateParameterName: not null })
        {
            _writer.WriteLine(
                $"Found duplicated parameter {parsingResult.DuplicateParameterName} for command {commandName}.");
                            
            return true;
        }
                        
        if (parsingResult is { Parsed: false, MissingRequiredParameterNames: not null })
        {
            _writer.WriteLine(
                $"Missing required parameters {string.Join(", ", parsingResult.MissingRequiredParameterNames)} for command {commandName}.");
                            
            return true;
        }
        
        return false;
    }

    private object CreateAndFillCommandInstance(CliCommand command, List<(string Name, string Value)> namedValues)
    {
        var commandInstance = Activator.CreateInstance(command.CommandType)!;

        foreach (var item in namedValues)
        {
            // property cannot be null here because this case covered by validation.
            var property = command.CommandType.GetProperty(item.Name)!;

            if (property.PropertyType == typeof(string))
            {
                property.SetValue(commandInstance, item.Value);
            }
            else
            {
                var convertedValue = Convert.ChangeType(item.Value, property.PropertyType, CultureInfo.InvariantCulture);
                property.SetValue(commandInstance, convertedValue);
            }
        }

        return commandInstance;
    }

    private void FindAndInvokeHandler(
        CliCommand command, 
        object commandInstance, 
        string unparsedCommand, 
        CancellationToken cancellationToken)
    {
        PropertyInfo? handlerProperty = command.GetType().GetProperty("Handler");
        
        if (handlerProperty != null)
        {
            // Handler registered via delegate.
            MethodInfo methodInfo = handlerProperty.PropertyType.GetMethod("Invoke")!;

            try
            {
                methodInfo.Invoke(
                    handlerProperty.GetValue(command, null),
                    [commandInstance, _writer, cancellationToken]);
            }
            catch (Exception ex)
            {
                _logWriter?.WriteLine($"Unexpected exception during command processing: {ex}");
            }

            return;
        }
        
        PropertyInfo? handlerFactoryProperty = command.GetType().GetProperty("HandlerFactory");
        
        if (handlerFactoryProperty != null)
        {
            // Handler registered via factory.
            MethodInfo methodInfo = handlerFactoryProperty.PropertyType.GetMethod("Invoke")!;

            var handlerInstance = methodInfo.Invoke(handlerFactoryProperty.GetValue(command, null), []);

            MethodInfo? handleMethod = handlerInstance!.GetType().GetMethod("Handle");

            try
            {
                handleMethod!.Invoke(handlerInstance, [commandInstance, _writer, cancellationToken]);
            }
            catch (Exception ex)
            {
                _logWriter?.WriteLine(
                    $"Unexpected exception during command processing: {unparsedCommand}. Exception: {ex}");
            }
            
            return;
        }
        
        // Likely there is not new inheritor of CliCommand.
        throw new NotImplementedException(
            $"Not implement FindAndInvokeHandler method for {command.GetType().ToString()}");
    }
}