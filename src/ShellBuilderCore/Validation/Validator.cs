using ShellBuilderCore.Command;

namespace ShellBuilderCore.Validation;

internal static class Validator
{
    public static void Validate(List<TextCommand> allCommands, bool generateHelpCommand)
    {
        // Как минимум должна быть зарегистрирована одна команда.
        if (allCommands.Count == 0)
        {
            throw new ValidationException(
                ValidationErrorCode.NoRegisteredCommands,
                "You should register at least one command.");
        }

        HashSet<string> inputCommands = new();
        
        foreach (var command in allCommands)
        {
            // Нельзя использовать зарезервированные команды при определенных условиях.
            if (generateHelpCommand && command.InputCommandTemplate == "help")
            {
                throw new ValidationException(
                    ValidationErrorCode.UsingReservedCommandName,
                    "If you use generated help command than you cannot use custom help command.");
            }

            var template = command.InputCommandTemplate?.Trim();

            if (string.IsNullOrEmpty(template))
            {
                throw new ValidationException(
                    ValidationErrorCode.InputTemplateIsNullOrEmpty,
                    "Template cannot be equals to empty string or null.");
            }
            
            // Все input templates уникальны между собой.
            if (inputCommands.TryGetValue(template, out _))
            {
                throw new ValidationException(
                    ValidationErrorCode.DuplicateInputTemplate,
                    $"Found duplicated template: {template}.");
            }

            inputCommands.Add(template);
            
            // Соответствие имен параметров в input template именам типа класса команды
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
}