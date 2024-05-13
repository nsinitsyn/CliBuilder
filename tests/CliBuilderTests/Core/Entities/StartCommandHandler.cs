using CliBuilderCore;

namespace CliBuilderTests.Core.Entities;

public class StartCommandHandler : ICommandHandler<StartCommand>
{
    public void Handle(StartCommand command, TextWriter writer, CancellationToken cancellationToken)
    {
        writer.WriteLine($"start work for url={command.Url} and thread count = {command.ThreadsCount}");
    }
}