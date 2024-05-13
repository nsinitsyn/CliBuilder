using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("CliBuilderTests")]

namespace CliBuilderCore.Parsing;

internal static class StringTemplateParser
{
    // Regex for search of parameters in ""
    private const string QuotesParametersPattern = "['\"][^\"]+['\"]";
    
    private const string RegexToken = "{{REGEX}}";

    private static readonly Regex QuotesParametersRegex = new(QuotesParametersPattern, RegexOptions.Compiled);
    
    public static bool TryParse(string input, string template, out List<(string Name, string Value)>? parsedItems)
    {
        var quotesValues = QuotesParametersRegex.Matches(input);
        var replacedInput = QuotesParametersRegex.Replace(input, RegexToken);
        
        var templateTokens = template.Split(" ");
        var inputTokens = replacedInput.Split(" ");

        if (templateTokens.Length != inputTokens.Length)
        {
            parsedItems = null;
            return false;
        }

        parsedItems = new();
        
        var regexMatchesIterator = 0;
        for (var i = 0; i < templateTokens.Length; i++)
        {
            if (templateTokens[i].Contains("[[") && templateTokens[i].Contains("]]"))
            {
                var name = templateTokens[i].Replace("[[", string.Empty).Replace("]]", string.Empty);
                
                var value = inputTokens[i] == RegexToken
                    ? quotesValues[regexMatchesIterator++].Value.Replace("\"", string.Empty)
                    : inputTokens[i];
                
                parsedItems.Add((name, value));
            }
            else
            {
                // Keyword
                if (string.Equals(templateTokens[i], inputTokens[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                parsedItems = null;
                return false;
            }
        }

        return true;
    }
}