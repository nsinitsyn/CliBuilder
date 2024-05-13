using Calculator.Commands;
using CliBuilderCore;

var cts = new CancellationTokenSource();

var cli = new CliBuilder()
    .RegisterCommand<TwoNumbersOperationCommand>("add [[A]] [[B]]", (cmd, writer, _) => { writer.WriteLine($"{cmd.A} + {cmd.B} = {cmd.A + cmd.B}"); })
    .RegisterCommand<TwoNumbersOperationCommand>("sub [[A]] [[B]]", (cmd, writer, _) => { writer.WriteLine($"{cmd.A} - {cmd.B} = {cmd.A - cmd.B}"); })
    .RegisterCommand<OneNumberOperationCommand>("sqrt [[A]]", (cmd, writer, _) => { writer.WriteLine($"Square root of {cmd.A} = {Math.Sqrt(cmd.A)}"); })
    .RegisterCommand<EmptyCommand>("exit", (_, _, _) => { cts.Cancel(); })
    .Build();

cli.Run(cts.Token);