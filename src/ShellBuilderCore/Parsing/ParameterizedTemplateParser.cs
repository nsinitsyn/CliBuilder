using System.Reflection;
using System.Text.RegularExpressions;
using ShellBuilderCore.Command;
using ShellBuilderCore.Command.Templates;

namespace ShellBuilderCore.Parsing;

internal static class ParameterizedTemplateParser
{
    // Регулярное выражение для поиска параметров, заключенных в кавычки.
    private const string QuotesParametersPattern = "['\"][a-zA-Z0-9-_=,.:;\\/ ]+['\"]";
    
    // Регулярное выражение для поиска параметров, заключенных в [[]]
    private const string ParametersNamesPattern = "\\[\\[[^[]*\\]\\]";

    private const string RegexToken = "{{REGEX}}";

    private static readonly Regex QuotesParametersRegex = new(QuotesParametersPattern, RegexOptions.Compiled);
    private static readonly Regex ParametersNamesRegex = new(ParametersNamesPattern, RegexOptions.Compiled);
    
    public static ParsingResult TryParse(string input, ParameterizedTemplate template, TextCommand cmd)
    {
        var quotesValues = QuotesParametersRegex.Matches(input);
        var replacedInput = QuotesParametersRegex.Replace(input, RegexToken);
        
        var inputTokens = replacedInput.Split(" ");

        var commandNameTokens = template.Name.Split(" ").Select(x => x.Trim()).ToList();

        var result = new ParsingResult
        {
            CommandMatchedByStartedKeywords = false
        };

        for (var i = 0; i < commandNameTokens.Count; i++)
        {
            if (i == inputTokens.Length)
            {
                // Начальных ключевых слов в template больше, чем было введено
                return result;
            }
            
            if (commandNameTokens[i] == inputTokens[i])
            {
                continue;
            }

            return result;
        }
        
        result.CommandMatchedByStartedKeywords = true;
        
        // Нужный шаблон найден. Теперь необходимо распарсить параметры.
        
        var commandInstance = Activator.CreateInstance(cmd.CommandType)!;

        bool parameterNameExpected = true;
        TemplateParameter? parameter = null;

        // Для отслеживания дубликатов параметров
        HashSet<string> handledParameterNames = new();
        
        var regexMatchesIterator = 0;
        for (var i = commandNameTokens.Count; i < inputTokens.Length; i++)
        {
            var token = inputTokens[i];

            if (parameterNameExpected)
            {
                // Ищем token среди имен и алиасов параметров шаблона
                parameter =
                    template.Parameters.FirstOrDefault(x => x.Name == token) ??
                    template.Parameters.FirstOrDefault(x => x.Alias == token);

                if (parameter == null)
                {
                    // Имя параметра не найдено в шаблоне, ошибка
                    result.NotFoundParameterName = token;
                    return result;
                }

                if (parameter.OnlyNameMappedBooleanPropertyName != null)
                {
                    if (!handledParameterNames.Add(parameter.Name))
                    {
                        result.DuplicateParameterName = parameter.Name;
                        return result;
                    }

                    var property = cmd.CommandType.GetProperty(parameter.OnlyNameMappedBooleanPropertyName!);
                    property!.SetValue(commandInstance, true);
                 
                    parameter = null;
                    continue;
                }

                parameterNameExpected = false;
                continue;
            }

            if (!parameter!.IsRepeatable && handledParameterNames.Contains(parameter.Name))
            {
                result.DuplicateParameterName = parameter.Name;
                return result;
            }

            handledParameterNames.Add(parameter.Name);
            
            // Проверка значения
            token = token == RegexToken
                ? quotesValues[regexMatchesIterator++].Value.Replace("\"", string.Empty)
                : token;

            var parameterNames = ParametersNamesRegex.Matches(parameter.ValueTemplate!).Select(x => x.Value).ToList();
            var pattern = ParametersNamesRegex.Replace(parameter.ValueTemplate!, "(.*)");
            
            // Используем жадный квантификатор, чтобы сначала найти минимальную строку соответствия (до первого разделителя, а не последнего)
            if (parameterNames.Count > 1)
            {
                var regex = new Regex(Regex.Escape("(.*)"), RegexOptions.Compiled);
                pattern = regex.Replace(pattern, "(.*?)", 1);
            }
            
            var match = Regex.Match(token, pattern);
            object? repeatableParameterItem = null;
            for (var j = 1; j < match.Groups.Count; j++)
            {
                var parameterName = parameterNames[j - 1].Replace("[[", string.Empty).Replace("]]", string.Empty);
                var parameterValue = match.Groups[j].Value;

                PropertyInfo property;
                
                if (parameter.CompositePropertyName != null)
                {
                    // Маппим на поля составного свойства
                    var compositeProperty = cmd.CommandType.GetProperty(parameter.CompositePropertyName);

                    if (parameter.IsRepeatable)
                    {
                        // Значит compositeProperty - наследник ICollection
                        var list = compositeProperty!.GetValue(commandInstance);
                        var listElementType = compositeProperty.PropertyType.GenericTypeArguments.First();
                        
                        // todo: вынести работу со списком здесь и ниже в общий метод
                        if (list == null)
                        {
                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(listElementType);

                            var instance = Activator.CreateInstance(constructedListType);
                            
                            compositeProperty.SetValue(commandInstance, instance);
                            list = instance;
                        }
                        
                        if (repeatableParameterItem == null)
                        {
                            repeatableParameterItem = Activator.CreateInstance(listElementType);
                            var listAddMethod = list!.GetType().GetMethod("Add");
                            listAddMethod!.Invoke(list, [repeatableParameterItem]);
                        }
                        
                        property = repeatableParameterItem!.GetType().GetProperty(parameterName)!;
                        var convertedValue = Convert.ChangeType(parameterValue, property.PropertyType);
                        property.SetValue(repeatableParameterItem, convertedValue);
                    }
                    else
                    {
                        var compositePropertyValue = compositeProperty!.GetValue(commandInstance);
                        if (compositePropertyValue == null)
                        {
                            compositePropertyValue = Activator.CreateInstance(compositeProperty.PropertyType);
                            compositeProperty.SetValue(commandInstance, compositePropertyValue);
                        }
                        
                        property = compositePropertyValue!.GetType().GetProperty(parameterName)!;
                        
                        var convertedValue = Convert.ChangeType(parameterValue, property!.PropertyType);
                        property.SetValue(compositePropertyValue, convertedValue);
                    }
                }
                else
                {
                    property = cmd.CommandType.GetProperty(parameterName)!;

                    if (parameter.IsRepeatable)
                    {
                        // Свойство - список стандартных типов: int, string и т.д.
                        
                        var list = property.GetValue(commandInstance);
                        var listElementType = property.PropertyType.GenericTypeArguments.First();
                        
                        if (list == null)
                        {
                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(listElementType);

                            var instance = Activator.CreateInstance(constructedListType);
                            
                            property.SetValue(commandInstance, instance);
                            list = instance;
                        }
                        
                        var listAddMethod = list!.GetType().GetMethod("Add");
                        var convertedValue = Convert.ChangeType(parameterValue, listElementType);
                        listAddMethod!.Invoke(list, [convertedValue]);
                    }
                    else
                    {
                        var convertedValue = Convert.ChangeType(parameterValue, property!.PropertyType);
                        property.SetValue(commandInstance, convertedValue);   
                    }
                }
            }

            parameterNameExpected = true;
            parameter = null;
        }
        
        // проверка, что все required параметры есть в введенной команде
        var missingRequiredParameters = template.Parameters.Where(x => x.IsRequired).Select(x => x.Name)
            .Except(handledParameterNames).ToList();

        if (missingRequiredParameters.Count != 0)
        {
            result.MissingRequiredParameterNames = missingRequiredParameters;
            return result;
        }

        result.Parsed = true;
        result.CommandInstance = commandInstance;
        return result;
    }
}