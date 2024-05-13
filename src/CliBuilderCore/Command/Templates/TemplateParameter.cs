namespace CliBuilderCore.Command.Templates;

internal class TemplateParameter
{
    public TemplateParameter(
        string name, 
        string? alias, 
        bool isRequired, 
        bool isRepeatable, 
        string? compositePropertyName, 
        string? valueTemplate,
        string? onlyNameMappedBooleanPropertyName,
        string? description = null)
    {
        Name = name;
        Alias = alias;
        IsRequired = isRequired;
        IsRepeatable = isRepeatable;
        CompositePropertyName = compositePropertyName;
        ValueTemplate = valueTemplate;
        OnlyNameMappedBooleanPropertyName = onlyNameMappedBooleanPropertyName;
        Description = description;
    }
    
    public string Name { get; set; }
    public string? Alias { get; set; }
    public bool IsRequired { get; set; }
    
    // If true than property type should be List<T>.
    public bool IsRepeatable { get; set; }
    
    // Type of this property List<T> or POCO-class.
    public string? CompositePropertyName { get; set; }
    
    public string? ValueTemplate { get; set; }

    public string? OnlyNameMappedBooleanPropertyName { get; set; }
    
    public string? Description { get; private set; }
}