namespace ShellBuilderCore.Command.Templates;

internal abstract class Template
{
    public TemplateType Type { get; private set; }

    public Template(TemplateType type)
    {
        Type = type;
    }
}