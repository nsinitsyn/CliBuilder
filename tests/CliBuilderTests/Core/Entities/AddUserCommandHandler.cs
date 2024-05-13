using CliBuilderCore;

namespace CliBuilderTests.Core.Entities;

public class AddUserCommandHandler : ICommandHandler<AddUserCommand>
{
    public void Handle(AddUserCommand command, TextWriter writer, CancellationToken cancellationToken)
    {
        writer.WriteLine($"added user: {command.Username} {command.Age} years old with address {command.Address}. Full name: {command.FullName}");
    }
}