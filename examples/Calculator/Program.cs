using Calculator.Commands;
using ShellBuilderCore;

var cts = new CancellationTokenSource();

var shell = new ShellBuilder()
    .RegisterCommand<TwoNumbersOperationCommand>("add [[A]] [[B]]", (cmd, writer, _) => { writer.WriteLine($"{cmd.A} + {cmd.B} = {cmd.A + cmd.B}"); })
    .RegisterCommand<TwoNumbersOperationCommand>("sub [[A]] [[B]]", (cmd, writer, _) => { writer.WriteLine($"{cmd.A} - {cmd.B} = {cmd.A - cmd.B}"); })
    .RegisterCommand<OneNumberOperationCommand>("sqrt [[A]]", (cmd, writer, _) => { writer.WriteLine($"Square root of {cmd.A} = {Math.Sqrt(cmd.A)}"); })
    .RegisterCommand<EmptyCommand>("exit", (_, _, _) => { cts.Cancel(); })
    .Build();

shell.Run(cts.Token);