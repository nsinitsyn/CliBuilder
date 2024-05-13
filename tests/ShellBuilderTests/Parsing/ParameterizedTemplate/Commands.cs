namespace ShellBuilderTests.Parsing.ParameterizedTemplate;

public class Command1
{
    public double Arg1 { get; set; }
    
    public string? Arg2 { get; set; }
}

public class Command2
{
    public double Arg1 { get; set; }
    
    public string? Arg2 { get; set; }
    
    public List<string>? Arg3 { get; set; }
    
    public bool Arg4 { get; set; }
    
    public Arg5? Arg5 { get; set; }
    
    public List<Arg6>? Arg6 { get; set; }
}

public class Arg5
{
    public int Var1 { get; set; }
    
    public string? Var2 { get; set; }
    
    public string? Var3 { get; set; }
}

public class Arg6
{
    public int ItemA { get; set; }
    
    public double ItemB { get; set; }
}