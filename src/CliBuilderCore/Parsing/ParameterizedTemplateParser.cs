using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CliBuilderCore.Command;
using CliBuilderCore.Command.Templates;

namespace CliBuilderCore.Parsing;

internal static class ParameterizedTemplateParser
{
    // Regex for search of parameters in ""
    private const string QuotesParametersPattern = "['\"][^\"]+['\"]";
    
    // Regex for search of parameters in [[]]
    private const string ParametersNamesPattern = "\\[\\[[^[]*\\]\\]";

    private const string RegexToken = "{{REGEX}}";

    private static readonly Regex QuotesParametersRegex = new(QuotesParametersPattern, RegexOptions.Compiled);
    private static readonly Regex ParametersNamesRegex = new(ParametersNamesPattern, RegexOptions.Compiled);
    
    public static ParsingResult TryParse(string input, ParameterizedTemplate template, CliCommand cmd)
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
                // Beginning keywords in template more than in input string.
                return result;
            }
            
            if (commandNameTokens[i] == inputTokens[i])
            {
                continue;
            }

            return result;
        }
        
        result.CommandMatchedByStartedKeywords = true;
        
        // Template was found. Now it need to parse parameters.
        
        var commandInstance = Activator.CreateInstance(cmd.CommandType)!;

        bool parameterNameExpected = true;
        TemplateParameter? parameter = null;

        // For find of duplicates parameters.
        HashSet<string> handledParameterNames = new();
        
        var regexMatchesIterator = 0;
        for (var i = commandNameTokens.Count; i < inputTokens.Length; i++)
        {
            var token = inputTokens[i];

            if (parameterNameExpected)
            {
                parameter =
                    template.Parameters.FirstOrDefault(x => x.Name == token) ??
                    template.Parameters.FirstOrDefault(x => x.Alias == token);

                if (parameter == null)
                {
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
            
            token = token == RegexToken
                ? quotesValues[regexMatchesIterator++].Value.Replace("\"", string.Empty)
                : token;

            var parameterNames = ParametersNamesRegex.Matches(parameter.ValueTemplate!).Select(x => x.Value).ToList();
            var pattern = ParametersNamesRegex.Replace(parameter.ValueTemplate!, "(.*)");
            
            // Using greedy quantifier that find minimum match string firstly (before first delimiter, not last).
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
                    // Map on properties of composite type.
                    var compositeProperty = cmd.CommandType.GetProperty(parameter.CompositePropertyName)!;

                    if (parameter.IsRepeatable)
                    {
                        // compositeProperty should be have the List type.
                        var (list, listElementType) = EnsureListPropertyInitialized(compositeProperty, commandInstance);
                        
                        if (repeatableParameterItem == null)
                        {
                            repeatableParameterItem = Activator.CreateInstance(listElementType);
                            var listAddMethod = list!.GetType().GetMethod("Add");
                            listAddMethod!.Invoke(list, [repeatableParameterItem]);
                        }
                        
                        property = repeatableParameterItem!.GetType().GetProperty(parameterName)!;
                        var convertedValue = Convert.ChangeType(parameterValue, property.PropertyType, CultureInfo.InvariantCulture);
                        property.SetValue(repeatableParameterItem, convertedValue);
                    }
                    else
                    {
                        var compositePropertyValue = compositeProperty.GetValue(commandInstance);
                        if (compositePropertyValue == null)
                        {
                            compositePropertyValue = Activator.CreateInstance(compositeProperty.PropertyType);
                            compositeProperty.SetValue(commandInstance, compositePropertyValue);
                        }
                        
                        property = compositePropertyValue!.GetType().GetProperty(parameterName)!;
                        
                        var convertedValue = Convert.ChangeType(parameterValue, property!.PropertyType, CultureInfo.InvariantCulture);
                        property.SetValue(compositePropertyValue, convertedValue);
                    }
                }
                else
                {
                    property = cmd.CommandType.GetProperty(parameterName)!;

                    if (parameter.IsRepeatable)
                    {
                        // Property - list of default types: int, string, etc.

                        var (list, listElementType) = EnsureListPropertyInitialized(property, commandInstance);
                        
                        var listAddMethod = list!.GetType().GetMethod("Add");
                        var convertedValue = Convert.ChangeType(parameterValue, listElementType, CultureInfo.InvariantCulture);
                        listAddMethod!.Invoke(list, [convertedValue]);
                    }
                    else
                    {
                        var convertedValue = Convert.ChangeType(parameterValue, property!.PropertyType, CultureInfo.InvariantCulture);
                        property.SetValue(commandInstance, convertedValue);   
                    }
                }
            }

            parameterNameExpected = true;
            parameter = null;
        }
        
        // Checking all required parameters exist in input string.
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

    private static (object, Type) EnsureListPropertyInitialized(PropertyInfo property, object commandInstance)
    {
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

        return (list!, listElementType);
    }
}