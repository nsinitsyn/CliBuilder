# CliBuilder
Simple library for fast creating command line utilities on .NET platform.

[![Build status](https://github.com/nsinitsyn/CliBuilder/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/nsinitsyn/CliBuilder/actions/workflows/dotnet.yml)

## Quick start

To use CliBuilder, you must create `CliBuilder` instance, setup this and call `Build` method for getting instance of `Cli` class. Then just call `Run` method. This method will be handled all input commands, parse these, create and fill instances of your command classes and call appropriate handlers.

You can create a Cli using the CliBuilder class as shown below:

<!-- snippet: quick-start -->
```cs
var cli = new CliBuilder()
    .RegisterCommand<AddCommand>(
        "add [[A]] [[B]]", // pattern for input string parsing
        (cmd, writer, _) => { writer.WriteLine($"{cmd.A} + {cmd.B} = {cmd.A + cmd.B}"); } // command handler
    )
    .Build();

cli.Run(CancellationToken.None);

// Command class
public class AddCommand
{
    public double A { get; set; } // [[A]] from input string will be assigned to A property
    public double B { get; set; } // [[B]] from input string will be assigned to B property
}
```
<!-- endSnippet -->

Just run this code in console application, type `add 10 5` and you have got next output: `10 + 5 = 15`

### Template and parameter builders

Previous sample used parameters without names and have the simple input pattern. 
In more complex cases you may need to setup parameters.

You can use `TemplateBuilder` and `TemplateParameterBuilder` in these cases. 

Sample for describe of some parameters of `docker run` command shown below:

<!-- snippet: quick-start-named-parameters -->
```cs
var runCommandTemplateBuilder = new TemplateBuilder()
    .WithName("docker run")
    .AddParameter(new TemplateParameterBuilder()
        .WithName("--name") // parameter name
        .WithAlias("-n") // alias
        .IsRequired() // is required
        .ValueTemplate("[[Name]]") // value template. Will be mapped to Name property of the command class
        .WithDescription("container name") // description for help generation
    )
    .AddParameter(new TemplateParameterBuilder()
        .WithName("-e") // this is parameter is optional because IsRequired calling missing
        .IsRepeatable() // parameter can be specified more than once
        .MapToCompositeProperty("EnvironmentVariables") // Will be mapped to property EnvironmentVariables of the command class
        .ValueTemplate("[[Name]]=[[Value]]") // Will be mapped to properties Name and Value of the property EnvironmentVariables class
        .WithDescription("environment variables")
    );

var cts = new CancellationTokenSource();
	
var cli = new CliBuilder()
    .RegisterCommand<DockerRunCommand>(runCommandTemplateBuilder, (cmd, _, _) => { /* cmd is instance of DockerRunCommand class */ }, "Run a command in a new container")
    .RegisterCommand<EmptyCommand>("exit", (_, _, _) => { cts.Cancel(); }) // exit command will stop input string waiting
    .SupportHelpCommand(generate: true) // help command will be generated
    .Build();

cli.Run(cts.Token);

// Command class
public class DockerRunCommand
{
    public string Name { get; set; }
    
    public List<EnvironmentVariable> EnvironmentVariables { get; set; }
}

public class EnvironmentVariable
{
    public string Name { get; set; }
    
    public string Value { get; set; }
}
```
<!-- endSnippet -->

Input value for this sample can be:

```sh
docker run --name my-container -e Variable1=Value1 -e Variable2=Value2
```

### Support double quotes

You must set the value with spaces in double quotes. For previous sample input value can be:

```sh
docker run --name my-container -e "ConnectionString=Host = postgres; Port=5432; Database = MyDB;"
```
### Two ways to register handlers

First way - using delegate as shown below:

<!-- snippet: quick-start-two-ways-first -->
```cs
var cli = new CliBuilder()
    .RegisterCommand<AddCommand>(
        "add [[A]] [[B]]",
        (cmd, writer, cancellationToken) => { writer.WriteLine($"{cmd.A} + {cmd.B} = {cmd.A + cmd.B}"); }
    )
    .Build();
```
<!-- endSnippet -->

Second way - implement of ICommandHandler interface and using factory as shown below:

<!-- snippet: quick-start-two-ways-second -->
```cs
var cli = new CliBuilder()
    .RegisterCommand("add [[A]] [[B]]", () => new AddCommandHandler())
    .Build();
	
public class AddCommandHandler : ICommandHandler<AddCommand>
{
    public void Handle(AddCommand command, TextWriter writer, CancellationToken cancellationToken)
    {
        writer.WriteLine($"{cmd.A} + {cmd.B} = {cmd.A + cmd.B}");
    }
}
```
<!-- endSnippet -->

### Abstraction from the console

By default CliBuilder using console in and out streams. But you can register any other streams, for example, for reading commands from file and write output to file as shown below:

<!-- snippet: quick-start-no-console -->
```cs
var cts = new CancellationTokenSource();

using FileStream inputFileStream = File.Open("input", FileMode.Open, FileAccess.Read);
using StreamReader inputReader = new StreamReader(inputFileStream);

using FileStream outputFileStream = File.Open("output", FileMode.OpenOrCreate, FileAccess.Write);
using StreamWriter outputWriter = new StreamWriter(outputFileStream);

var cli = new CliBuilder()
    .ReadFrom(inputReader)
    .WriteTo(outputWriter)
    .RegisterCommand<AddCommand>("add [[A]] [[B]]", (cmd, writer, _) => { writer.WriteLine($"{cmd.A} + {cmd.B} = {cmd.A + cmd.B}"); })
    .RegisterCommand<EmptyCommand>("exit", (_, _, _) => { cts.Cancel(); })
    .Build();

cli.Run(cts.Token);

/*
 input file:
 add 10 5
 add 20 30
 exit
*/
```
<!-- endSnippet -->

### Help generation

For help generation use method `SupportHelpCommand` with `true` argument:
<!-- snippet: quick-start-help-generation -->
```cs
var cli = new CliBuilder()
    // Commands registration block...
    .SupportHelpCommand(generate: true)
    .Build();
```
<!-- endSnippet -->

For example, below is the generated help output for Docker commands. Full sample you can find in `\examples\DockerCli`.
```
run
run a command in a new container
Parameters:
--name          -n      container name                                             
-e                      environment variables                                      
-v                      volumes                                                    
-d                      description                                                
-p                      ports mapping                                              
--restart               restart type                                               
--network               network name                                               
--rm                    automatically remove the container when it exits           

build
build an image from a Dockerfile
Parameters:
--file          -f      name of the Dockerfile                                     
--tag           -t      name and optionally a tag in the name:tag format           
--network               network name                                               
--rm                    remove intermediate containers after a successful build    

exit
exit from application
```
