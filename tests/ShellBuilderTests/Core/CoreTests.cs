using ShellBuilderCore;
using ShellBuilderTests.Core.Entities;

namespace ShellBuilderTests.Core;

public class CoreTests
{
    [Test]
    [Timeout(2000)]
    public void TestSimpleCase()
    {
        TextReader stringReader =
            new StringReader($"start http://site.com 4{Environment.NewLine}stop 123{Environment.NewLine}");

        TextWriter stringWriter = new StringWriter();
        
        var shell = new ShellBuilder()
            .ReadFrom(stringReader)
            .WriteTo(stringWriter)
            .RegisterCommand("start [[Url]] [[ThreadsCount]]", () => new StartCommandHandler())
            .RegisterCommand<StopCommand>(
                "stop [[Id]]",
                (command, writer, _) => { writer.WriteLine($"stop work with id = {command.Id}"); })
            .Build();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        shell.Run(cts.Token);

        var output = stringWriter.ToString();

        Assert.That(output,
            Is.EqualTo(
                $"start work for url=http://site.com and thread count = 4{Environment.NewLine}stop work with id = 123{Environment.NewLine}"));
    }
    
    [Test]
    [Timeout(2000)]
    public void TestWithQuotes()
    {
        TextReader stringReader =
            new StringReader($"add user Alex 32 \"Alexander Smith\" \"132, My Street, Kingston, New York 12401\"{Environment.NewLine}block user \"Alex\" \"There is a suspicion of spam.\"{Environment.NewLine}");

        TextWriter stringWriter = new StringWriter();
        
        var shell = new ShellBuilder()
            .ReadFrom(stringReader)
            .WriteTo(stringWriter)
            .RegisterCommand("add user [[Username]] [[Age]] [[FullName]] [[Address]]", () => new AddUserCommandHandler())
            .RegisterCommand("block user [[Username]] [[Reason]]", () => new BlockUserCommandHandler())
            .Build();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        shell.Run(cts.Token);

        var output = stringWriter.ToString();

        Assert.That(output,
            Is.EqualTo(
                $"added user: Alex 32 years old with address 132, My Street, Kingston, New York 12401. Full name: Alexander Smith{Environment.NewLine}blocked user Alex by reason: There is a suspicion of spam.{Environment.NewLine}"));
    }
}