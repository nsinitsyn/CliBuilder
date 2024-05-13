using CliBuilderCore.Command.Templates;

namespace CliBuilderCore.CommandBuilding;

public class TemplateBuilder
{
    private string _name;
    private List<TemplateParameter> _parameters = new();
    
    public TemplateBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public TemplateBuilder AddParameter(TemplateParameterBuilder parameterBuilder)
    {
        _parameters.Add(parameterBuilder.Build());
        return this;
    }

    internal ParameterizedTemplate Build()
    {
        return new ParameterizedTemplate(_name, _parameters);
    }
}