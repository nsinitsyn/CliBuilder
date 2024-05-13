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

You can use `TemplateBuilder` in these cases. 

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