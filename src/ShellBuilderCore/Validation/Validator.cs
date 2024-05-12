using ShellBuilderCore.Command;
using ShellBuilderCore.Command.Templates;

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
            if (command.Template.Type == TemplateType.String)
            {
                StringTemplateCommandValidator.Validate(command, generateHelpCommand, inputCommands);
            }
            else if (command.Template.Type == TemplateType.Parameterized)
            {
                // todo: валидация
                // уникальность определяется именами шаблона (напр, docker run)
                // Для одного шаблона имена параметров и алиасы уникальны
                // Все OnlyNameMappedBooleanPropertyName имеют тип bool
                // OnlyNameMappedBooleanPropertyName не могут быть IsRepeatable
            }
        }
    }
}