namespace ShellBuilderCore.Parsing;

internal class ParsingResult
{
    public bool CommandMatchedByStartedKeywords { get; set; }
    
    public bool Parsed { get; set; }
    
    public string? NotFoundParameterName { get; set; }
    
    public string? DuplicateParameterName { get; set; }
    
    public List<string>? MissingRequiredParameterNames { get; set; }
    
    public object? CommandInstance { get; set; }
}