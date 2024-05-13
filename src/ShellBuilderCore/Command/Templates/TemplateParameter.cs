namespace ShellBuilderCore.Command.Templates;

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
    public bool IsRepeatable { get; set; } // Если true, то тип свойства обязательно List<T>
    
    // Имя составного свойства, на которое мапим
    // напр, EnvironmentVariables или Description
    // Тип такого свойства всегда либо ICollection + наследники, либо POCO-класс
    public string? CompositePropertyName { get; set; }
    
    // Пробелы запрещены
    // [[Name]] - если MapToField = null, то мапим на поля класса команды
    // [[PortExternal]]:[[PortInternal]] - если MapToField = null, то мапим на поля класса команды
    // [[Name]]=[[Value]] - в этом случае MapToField = EnvironmentVariables. Значит мапим на поля класса свойства
    // [[Name]]:[[Tag]] - мапим на поля свойства Description в класс Description, т.к. MapToField = Description
    // если null, то допускается только имя и мапится на тип bool
    public string? ValueTemplate { get; set; }

    public string? OnlyNameMappedBooleanPropertyName { get; set; }
    
    public string? Description { get; private set; }
    
    // docker run {{-p:optional}} [[PortExternal]]:[[PortInternal]] {{--forced:field=Forced}} {{--name:required:alias=-n}} [[Name]] {{-e:optional:repeatable:field=EnvironmentVariables}} [[Name]]=[[Value]]
}