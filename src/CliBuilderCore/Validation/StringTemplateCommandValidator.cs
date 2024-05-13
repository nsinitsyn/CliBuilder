using CliBuilderCore.Command;
using CliBuilderCore.Command.Templates;

namespace CliBuilderCore.Validation;

internal static class StringTemplateCommandValidator
{
    public static void Validate(CliCommand command, bool generateHelpCommand, HashSet<string> inputCommands)
    {
        var stringTemplate = (StringTemplate)command.Template;
        
        if (generateHelpCommand && stringTemplate.InputString == "help")
        {
            throw new ValidationException(
                ValidationErrorCode.UsingReservedCommandName,
                "If you use generated help command than you cannot use custom help command.");
        }

        var template = stringTemplate.InputString?.Trim();

        if (string.IsNullOrEmpty(template))
        {
            throw new ValidationException(
                ValidationErrorCode.InputTemplateIsNullOrEmpty,
                "Template cannot be equals to empty string or null.");
        }
            
        if (inputCommands.TryGetValue(template, out _))
        {
            throw new ValidationException(
                ValidationErrorCode.DuplicateInputTemplate,
                $"Found duplicated template: {template}.");
        }

        inputCommands.Add(template);
            
        var tokens = template.Split(" ").Select(x => x.Trim());
        foreach (var token in tokens)
        {
            if (token.StartsWith("[[") && token.EndsWith("]]"))
            {
                var parameterName = token.Replace("[[", string.Empty).Replace("]]", string.Empty);
                    
                var property = command.CommandType.GetProperty(parameterName);
                    
                if (property == null)
                {
                    throw new ValidationException(
                        ValidationErrorCode.MissingPropertyInCommandClass,
                        $"Missing property {parameterName} in command {command.CommandType.ToString()}");
                }
            }
        }
    }
}