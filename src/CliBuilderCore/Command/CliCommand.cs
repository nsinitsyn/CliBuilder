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
    
    public string GenerateHelp()
    {
        var descriptionBlock = !string.IsNullOrEmpty(_description)
            ? $"Description: {_description}{Environment.NewLine}"
            : string.Empty;
        
        var parametersBlock = !string.IsNullOrEmpty(Template.HelpParameters)
            ? $"Parameters:{Environment.NewLine}{Template.HelpParameters}{Environment.NewLine}"
            : string.Empty;

        return
            $"{Environment.NewLine}Command: {Template.HelpHeader}{Environment.NewLine}{descriptionBlock}{parametersBlock}";
    }
}