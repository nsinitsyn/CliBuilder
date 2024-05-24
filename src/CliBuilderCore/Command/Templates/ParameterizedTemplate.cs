using System.Text;

namespace CliBuilderCore.Command.Templates;

internal class ParameterizedTemplate : Template
{
    public string Name { get; private set; }
    public List<TemplateParameter> Parameters { get; private set; }
    
    
    public ParameterizedTemplate(string name, List<TemplateParameter> parameters) 
        : base(TemplateType.Parameterized)
    {
        Name = name;
        Parameters = parameters;
    }


    public override string HelpHeader => Name;

    public override string HelpParameters(List<int> itemsLengths) =>
        string.Join(
            Environment.NewLine,
            Parameters.Select(x =>
                $"{AddSpaces(x.Name, itemsLengths[0])}\t{AddSpaces(x.Alias, itemsLengths[1])}\t{AddSpaces(x.Description, itemsLengths[2])}"));

    public override List<List<int>> HelpItemsLengths =>
        Parameters
            .Select(x => new List<int> { x.Name.Length, x.Alias?.Length ?? 0, x.Description?.Length ?? 0 })
            .ToList();

    private string? AddSpaces(string? item, int expectedLength)
    {
        if (item == null)
        {
            return null;
        }
        
        var spacesCount = expectedLength - item.Length;
        
        if (spacesCount == 0)
        {
            return item;
        }

        var builder = new StringBuilder(item);
        for (var i = 0; i < spacesCount; i++)
        {
            builder.Append(' ');
        }

        return builder.ToString();
    }
}