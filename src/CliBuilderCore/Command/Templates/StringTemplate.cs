namespace CliBuilderCore.Command.Templates;

internal class StringTemplate : Template
{
    public string InputString { get; private set; }

    public StringTemplate(string inputString) 
        : base(TemplateType.String)
    {
        InputString = inputString;
    }

    public override string HelpHeader => InputString;
    public override string HelpParameters(List<int> itemsLengths) => string.Empty;
}