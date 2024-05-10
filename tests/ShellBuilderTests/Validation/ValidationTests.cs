using ShellBuilderCore;
using ShellBuilderCore.Validation;

namespace ShellBuilderTests.Validation;

public class ValidationTests
{
    [Test]
    public void NoRegisteredCommands_Test()
    {
        var builder = new ShellBuilder();
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.NoRegisteredCommands));
    }
    
    [Test]
    public void UsingReservedCommandName_Test()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>("help", (_, _, _) => { })
            .SupportHelpCommand(generate: true);
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.UsingReservedCommandName));
    }
    
    [Test]
    public void InputTemplateIsNullOrEmpty_Test()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>("", (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.InputTemplateIsNullOrEmpty));
    }
    
    [Test]
    public void DuplicateInputTemplate_Test()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>("run", (_, _, _) => { })
            .RegisterCommand<EmptyCommand>("run", (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.DuplicateInputTemplate));
    }
    
    [Test]
    public void MissingPropertyInCommandClass()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>("download [[Url]]", (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.MissingPropertyInCommandClass));
    }
}