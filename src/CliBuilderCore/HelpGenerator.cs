using CliBuilderCore.Command;
using CliBuilderCore.Command.Templates;

namespace CliBuilderCore;

internal static class HelpGenerator
{
    public static string Generate(List<CliCommand> commands)
    {
        // Хранит максимальные длины значений параметров справки.
        List<int> maxLengths = new();
        
        // Вычисляем maxLengths
        foreach (var command in commands)
        {
            var lengths = command.Template.HelpItemsLengths;
            
            if (lengths.Count != 0)
            {
                foreach (var lineLengths in lengths)
                {
                    if (maxLengths.Count == 0)
                    {
                        maxLengths = Enumerable.Repeat(0, lineLengths.Count).ToList();
                    }
                    
                    for (int i = 0; i < lineLengths.Count; i++)
                    {
                        if (maxLengths[i] < lineLengths[i])
                        {
                            maxLengths[i] = lineLengths[i];
                        }
                    }
                }
            }
        }

        // Каждому значению параметра в строке n в справке будут добавлены пробелы так,
        // чтобы длина параметра стала равняться actualLengths[n]. Кроме последнего столбца.
        List<int> actualLengths = maxLengths.Select(x => x + 4).ToList();

        var helpOutput = string.Join(string.Empty, commands.Select(x => x.GenerateHelp(actualLengths)));
        return helpOutput;
    }
}