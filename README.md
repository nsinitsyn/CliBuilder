# CliBuilder
Simple helper for fast creating command line utilities on .NET platform.

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