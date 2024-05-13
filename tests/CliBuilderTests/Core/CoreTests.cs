using CliBuilderCore;
using CliBuilderCore.CommandBuilding;
using CliBuilderTests.Core.Entities;

namespace CliBuilderTests.Core;

public class CoreTests
{
    [Test]
    [Timeout(2000)]
    public void TestSimpleCase()
    {
        TextReader stringReader =
            new StringReader($"start http://site.com 4{Environment.NewLine}stop 123{Environment.NewLine}");

        TextWriter stringWriter = new StringWriter();
        
        var cli = new CliBuilder()
            .ReadFrom(stringReader)
            .WriteTo(stringWriter)
            .RegisterCommand("start [[Url]] [[ThreadsCount]]", () => new StartCommandHandler())
            .RegisterCommand<StopCommand>(
                "stop [[Id]]",
                (command, writer, _) => { writer.WriteLine($"stop work with id = {command.Id}"); })
            .Build();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        cli.Run(cts.Token);

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
        
        var cli = new CliBuilder()
            .ReadFrom(stringReader)
            .WriteTo(stringWriter)
            .RegisterCommand("add user [[Username]] [[Age]] [[FullName]] [[Address]]", () => new AddUserCommandHandler())
            .RegisterCommand("block user [[Username]] [[Reason]]", () => new BlockUserCommandHandler())
            .Build();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        cli.Run(cts.Token);

        var output = stringWriter.ToString();

        Assert.That(output,
            Is.EqualTo(
                $"added user: Alex 32 years old with address 132, My Street, Kingston, New York 12401. Full name: Alexander Smith{Environment.NewLine}blocked user Alex by reason: There is a suspicion of spam.{Environment.NewLine}"));
    }
    
    [Test]
    [Timeout(2000)]
    public void TestParameterizedTemplate()
    {
        TextReader stringReader =
            new StringReader($"docker run --rm -p 25000:40000 -d my-service:v1 --name my-service -e \"User=Alex Smith\" -e \"Kestrel__Endpoints__Http__Url=http://0.0.0.0:40000\" -e \"ConnectionStrings__Default=Host=postgres;Port=5432;Database=MyDB;Username=postgres;Password=123123;SSL Mode=Disable;\" -v /home/ubuntu/Logs/my-app:/app/Log -v /etc/timezone:/etc/timezone:ro -v /etc/localtime:/etc/localtime:ro --restart unless-stopped --network my-network{Environment.NewLine}");
    
        TextWriter stringWriter = new StringWriter();
        
        var commandTemplateBuilder = new TemplateBuilder()
            .WithName("docker run")
            .AddParameter(new TemplateParameterBuilder()
                .WithName("--name")
                .WithAlias("-n")
                .IsRequired()
                .ValueTemplate("[[Name]]")
            )
            .AddParameter(new TemplateParameterBuilder()
                .WithName("-e")
                .IsRepeatable()
                .MapToCompositeProperty("EnvironmentVariables")
                .ValueTemplate("[[Name]]=[[Value]]")
            )
            .AddParameter(new TemplateParameterBuilder()
                .WithName("-v")
                .IsRepeatable()
                .MapToCompositeProperty("Volumes")
                .ValueTemplate("[[Name]]:[[MapTo]]")
            )
            .AddParameter(new TemplateParameterBuilder()
                .WithName("-d")
                .IsRequired()
                .MapToCompositeProperty("Description")
                .ValueTemplate("[[Name]]:[[Tag]]")
            )
            .AddParameter(new TemplateParameterBuilder()
                .WithName("-p")
                .ValueTemplate("[[PortExternal]]:[[PortInternal]]")
            )
            .AddParameter(new TemplateParameterBuilder()
                .WithName("--restart")
                .ValueTemplate("[[RestartType]]")
            )
            .AddParameter(new TemplateParameterBuilder()
                .WithName("--network")
                .ValueTemplate("[[Network]]")
            )
            .AddParameter(new TemplateParameterBuilder()
                .WithName("--rm")
                .OnlyName("Removed")
            );

        var cts = new CancellationTokenSource();
        DockerRunCommand? actualCommand = null;

        var cli = new CliBuilder()
            .ReadFrom(stringReader)
            .WriteTo(stringWriter)
            .RegisterCommand<DockerRunCommand>(commandTemplateBuilder, (cmd, _, _) =>
            {
                actualCommand = cmd;
                cts.Cancel();
            })
            .Build();
    
        cli.Run(cts.Token);
        
        Assert.That(actualCommand, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(actualCommand!.Name, Is.EqualTo("my-service"));
            Assert.That(actualCommand.Network, Is.EqualTo("my-network"));
            Assert.That(actualCommand.Removed, Is.True);
            Assert.That(actualCommand.PortExternal, Is.EqualTo(25000));
            Assert.That(actualCommand.PortInternal, Is.EqualTo(40000));
            Assert.That(actualCommand.RestartType, Is.EqualTo("unless-stopped"));
            Assert.That(actualCommand.Description.Name, Is.EqualTo("my-service"));
            Assert.That(actualCommand.Description.Tag, Is.EqualTo("v1"));
            
            Assert.That(actualCommand.EnvironmentVariables, Has.Count.EqualTo(3));
            Assert.That(actualCommand.EnvironmentVariables[0].Name, Is.EqualTo("User"));
            Assert.That(actualCommand.EnvironmentVariables[0].Value, Is.EqualTo("Alex Smith"));
            Assert.That(actualCommand.EnvironmentVariables[1].Name, Is.EqualTo("Kestrel__Endpoints__Http__Url"));
            Assert.That(actualCommand.EnvironmentVariables[1].Value, Is.EqualTo("http://0.0.0.0:40000"));
            Assert.That(actualCommand.EnvironmentVariables[2].Name, Is.EqualTo("ConnectionStrings__Default"));
            Assert.That(actualCommand.EnvironmentVariables[2].Value, Is.EqualTo("Host=postgres;Port=5432;Database=MyDB;Username=postgres;Password=123123;SSL Mode=Disable;"));
            
            Assert.That(actualCommand.Volumes, Has.Count.EqualTo(3));
            Assert.That(actualCommand.Volumes[0].Name, Is.EqualTo("/home/ubuntu/Logs/my-app"));
            Assert.That(actualCommand.Volumes[0].MapTo, Is.EqualTo("/app/Log"));
            Assert.That(actualCommand.Volumes[1].Name, Is.EqualTo("/etc/timezone"));
            Assert.That(actualCommand.Volumes[1].MapTo, Is.EqualTo("/etc/timezone:ro"));
            Assert.That(actualCommand.Volumes[2].Name, Is.EqualTo("/etc/localtime"));
            Assert.That(actualCommand.Volumes[2].MapTo, Is.EqualTo("/etc/localtime:ro"));
        });
    }
}