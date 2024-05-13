using ShellBuilderCore.Command.Templates;

namespace ShellBuilderCore.Command;

internal abstract class TextCommand
{
    public TextCommand(Type commandType, Template template, string? description)
    {
        CommandType = commandType;
        Template = template;
        Description = description;
    }
    
    public Type CommandType { get; private set; }

    public Template Template { get; private set; }

    public string? Description { get; private set; }

    public string GenerateHelp()
    {
        var descriptionBlock = !string.IsNullOrEmpty(Description)
            ? $"Description: {Description}{Environment.NewLine}"
            : string.Empty;
        
        var parametersBlock = !string.IsNullOrEmpty(Template.HelpParameters)
            ? $"Parameters:{Environment.NewLine}{Template.HelpParameters}{Environment.NewLine}"
            : string.Empty;

        return
            $"{Environment.NewLine}Command: {Template.HelpHeader}{Environment.NewLine}{descriptionBlock}{parametersBlock}";
    }
}