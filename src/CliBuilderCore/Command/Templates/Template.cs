namespace CliBuilderCore.Command.Templates;

internal abstract class Template
{
    public abstract string HelpHeader { get; }
    public abstract string HelpParameters(List<int> itemsLengths);

    /// <summary>
    /// Список длин текстовых значений в справке для каждой строки.
    /// Используется для расчета выравнивания столбцов в логике генерации справки.
    /// Внешний список - параметры шаблона (строки).
    /// Внутренний список - значения параметров (столбцы).
    /// </summary>
    public virtual List<List<int>> HelpItemsLengths { get; } = new();
    
    public TemplateType Type { get; private set; }

    public Template(TemplateType type)
    {
        Type = type;
    }
}