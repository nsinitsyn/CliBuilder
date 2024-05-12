namespace ShellBuilderCore.Command.Templates;

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
}