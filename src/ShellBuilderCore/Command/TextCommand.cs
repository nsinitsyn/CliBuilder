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
}