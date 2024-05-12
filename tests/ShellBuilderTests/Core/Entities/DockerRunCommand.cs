namespace ShellBuilderTests.Core.Entities;

public class DockerRunCommand
{
    public string Name { get; set; }
    
    public List<EnvironmentVariable> EnvironmentVariables { get; set; }
    
    public int PortInternal { get; set; }
    
    public int PortExternal { get; set; }
    
    public Description Description { get; set; }
    
    public bool Removed { get; set; }
    
    public List<Volume> Volumes { get; set; }
    
    public string RestartType { get; set; }
    
    public string Network { get; set; }
}

public class EnvironmentVariable
{
    public string Name { get; set; }
    
    public string Value { get; set; }
}

public class Description
{
    public string Name { get; set; }
    
    public string Tag { get; set; }
}

public class Volume
{
    public string Name { get; set; }
    
    public string MapTo { get; set; }
}