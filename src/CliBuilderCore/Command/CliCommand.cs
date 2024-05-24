using CliBuilderCore.Command.Templates;

namespace CliBuilderCore.Command;

internal abstract class CliCommand
{
    private readonly string? _description;

    public CliCommand(Type commandType, Template template, string? description)
    {
        CommandType = commandType;
        Template = template;
        _description = description;
    }
    
    public Type CommandType { get; private set; }

    public Template Template { get; private set; }
    
    public string GenerateHelp(List<int> itemsLengths)
    {
        var descriptionBlock = !string.IsNullOrEmpty(_description)
            ? $"{_description}{Environment.NewLine}"
            : string.Empty;
        
        var parametersBlock = !string.IsNullOrEmpty(Template.HelpParameters(itemsLengths))
            ? $"Parameters:{Environment.NewLine}{Template.HelpParameters(itemsLengths)}{Environment.NewLine}"
            : string.Empty;

        return
            $"{Environment.NewLine}{Template.HelpHeader}{Environment.NewLine}{descriptionBlock}{parametersBlock}";
    }
}