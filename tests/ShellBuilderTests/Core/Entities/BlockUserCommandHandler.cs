using ShellBuilderCore;

namespace ShellBuilderTests.Core.Entities;

public class BlockUserCommandHandler : ICommandHandler<BlockUserCommand>
{
    public void Handle(BlockUserCommand command, TextWriter writer, CancellationToken cancellationToken)
    {
        writer.WriteLine($"blocked user {command.Username} by reason: {command.Reason}");
    }
}