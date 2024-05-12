using ShellBuilderCore.Command.Templates;

namespace ShellBuilderCore.CommandBuilding;

public class TemplateParameterBuilder
{
    private string _name = string.Empty;
    private string? _alias;
    private bool _isRequired = false;
    private bool _isRepeatable = false;
    private string? _compositePropertyName;
    private string? _valueTemplate;
    private string? _onlyNameMappedBooleanPropertyName;
    
    public TemplateParameterBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public TemplateParameterBuilder WithAlias(string alias)
    {
        _alias = alias;
        return this;
    }
    
    public TemplateParameterBuilder IsRequired()
    {
        _isRequired = true;
        return this;
    }
    
    public TemplateParameterBuilder IsRepeatable()
    {
        _isRepeatable = true;
        return this;
    }
    
    public TemplateParameterBuilder MapToCompositeProperty(string propertyName)
    {
        _compositePropertyName = propertyName;
        return this;
    }
    
    public TemplateParameterBuilder ValueTemplate(string valueTemplate)
    {
        _valueTemplate = valueTemplate;
        return this;
    }

    // Параметр без значения - опциональный - либо указан, либо нет. Не может использоваться с IsRequired=true. Пример: --forced 
    // Указывается имя булева свойства, на которое мапится.
    public TemplateParameterBuilder OnlyName(string mappedBooleanPropertyName)
    {
        _onlyNameMappedBooleanPropertyName = mappedBooleanPropertyName;
        return this;
    }

    internal TemplateParameter Build()
    {
        return new TemplateParameter(
            _name,
            _alias,
            _isRequired,
            _isRepeatable,
            _compositePropertyName,
            _valueTemplate,
            _onlyNameMappedBooleanPropertyName);
    }
}