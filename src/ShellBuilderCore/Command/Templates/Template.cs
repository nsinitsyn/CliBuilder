namespace ShellBuilderCore.Command.Templates;

internal abstract class Template
{
    public abstract string HelpHeader { get; }
    public abstract string HelpParameters { get; }

    public TemplateType Type { get; private set; }

    public Template(TemplateType type)
    {
        Type = type;
    }
}