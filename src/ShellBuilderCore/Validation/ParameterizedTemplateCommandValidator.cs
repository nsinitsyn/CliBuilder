using ShellBuilderCore.Command;
using ShellBuilderCore.Command.Templates;

namespace ShellBuilderCore.Validation;

internal static class ParameterizedTemplateCommandValidator
{
    public static void Validate(TextCommand command, bool generateHelpCommand, HashSet<string> inputCommands)
    {
        var template = (ParameterizedTemplate)command.Template;
        
        // Нельзя использовать зарезервированные команды при определенных условиях.
        if (generateHelpCommand && template.Name == "help")
        {
            throw new ValidationException(
                ValidationErrorCode.UsingReservedCommandName,
                "If you use generated help command than you cannot use custom help command.");
        }
        
        if (string.IsNullOrEmpty(template.Name))
        {
            throw new ValidationException(
                ValidationErrorCode.InputTemplateIsNullOrEmpty,
                "Template cannot be equals to empty string or null.");
        }
        
        // Уникальность определяется именами шаблона (напр, docker run)
        if (inputCommands.TryGetValue(template.Name, out _))
        {
            throw new ValidationException(
                ValidationErrorCode.DuplicateInputTemplate,
                $"Found duplicated template: {template.Name}.");
        }

        inputCommands.Add(template.Name);
        
        // Для одного шаблона имена параметров и алиасы уникальны
        HashSet<string> parameterNames = new();
        foreach (var parameter in template.Parameters)
        {
            if (!parameterNames.Add(parameter.Name))
            {
                throw new ValidationException(
                    ValidationErrorCode.DuplicateParameterName,
                    $"Found duplicated parameter name: {parameter.Name} for template {template.Name}.");
            }
            
            if (parameter.Alias != null && !parameterNames.Add(parameter.Alias))
            {
                throw new ValidationException(
                    ValidationErrorCode.DuplicateParameterName,
                    $"Found duplicated parameter name: {parameter.Alias} for template {template.Name}.");
            }

            if (parameter.OnlyNameMappedBooleanPropertyName != null)
            {
                if (parameter.IsRepeatable)
                {
                    // OnlyNameMappedBooleanPropertyName не могут быть IsRepeatable
                    throw new ValidationException(
                        ValidationErrorCode.RepeatableOnlyNameParameter,
                        $"Only-named parameter cannot be repeatable. Parameter name: {parameter.Name}. Template {template.Name}.");
                }
                
                if (parameter.IsRequired)
                {
                    // OnlyNameMappedBooleanPropertyName не могут быть IsRequired
                    throw new ValidationException(
                        ValidationErrorCode.RequiredOnlyNameParameter,
                        $"Only-named parameter cannot be required. Parameter name: {parameter.Name}. Template {template.Name}.");
                }
                
                // Все OnlyNameMappedBooleanPropertyName имеют тип bool
                if (command.CommandType.GetProperty(parameter.OnlyNameMappedBooleanPropertyName).PropertyType !=
                    typeof(bool))
                {
                    throw new ValidationException(
                        ValidationErrorCode.OnlyNameParameterTypeNotBool,
                        $"Only-named parameter should be boolean type. Parameter name: {parameter.Name}. Template {template.Name}.");
                }
            }
        }
    }   
}