namespace ShellBuilderCore.Command;

internal abstract class TextCommand
{
    public TextCommand(Type commandType, string inputCommandTemplate, string? description)
    {
        CommandType = commandType;
        InputCommandTemplate = inputCommandTemplate;
        Description = description;
    }
    
    public Type CommandType { get; private set; }

    public string InputCommandTemplate { get; private set; }

    public string? Description { get; private set; }
}