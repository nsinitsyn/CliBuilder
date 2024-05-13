namespace DockerCli.Commands;

public class BuildCommand
{
    public string DockerfileName { get; set; }
    
    public string Network { get; set; }
    
    public bool Removed { get; set; }
    
    public string Name { get; set; }
    
    public string Tag { get; set; }
}