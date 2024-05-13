using ShellBuilderCore;
using ShellBuilderCore.CommandBuilding;
using ShellBuilderCore.Validation;
using ShellBuilderTests.Validation.Entities;

namespace ShellBuilderTests.Validation;

public class ValidationTests
{
    [Test]
    public void NoRegisteredCommands()
    {
        var builder = new ShellBuilder();
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.NoRegisteredCommands));
    }
    
    [Test]
    public void UsingReservedCommandName()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>("help", (_, _, _) => { })
            .SupportHelpCommand(generate: true);
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.UsingReservedCommandName));
    }
    
    [Test]
    public void InputTemplateIsNullOrEmpty()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>("", (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.InputTemplateIsNullOrEmpty));
    }
    
    [Test]
    public void DuplicateInputTemplate()
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

    [Test]
    public void DuplicateParameterName()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>(
                new TemplateBuilder()
                    .WithName("run")
                    .AddParameter(new TemplateParameterBuilder().WithName("-a"))
                    .AddParameter(new TemplateParameterBuilder().WithName("--a").WithAlias("-a")),
                (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.DuplicateParameterName));
    }
    
    [Test]
    public void RepeatableOnlyNameParameter()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>(
                new TemplateBuilder()
                    .WithName("run")
                    .AddParameter(new TemplateParameterBuilder().WithName("-name").IsRepeatable().OnlyName("Name")),
                (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.RepeatableOnlyNameParameter));
    }
    
    [Test]
    public void RequiredOnlyNameParameter()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>(
                new TemplateBuilder()
                    .WithName("run")
                    .AddParameter(new TemplateParameterBuilder().WithName("-name").IsRequired().OnlyName("Name")),
                (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.RequiredOnlyNameParameter));
    }
    
    [Test]
    public void OnlyNameParameterTypeNotBool()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<OnlyNameParameterTypeNotBoolTestCommand>(
                new TemplateBuilder()
                    .WithName("run")
                    .AddParameter(new TemplateParameterBuilder().WithName("-param").OnlyName("Param")),
                (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.OnlyNameParameterTypeNotBool));
    }
    
    [Test]
    public void DuplicateInputTemplateWithParameterizedTemplate()
    {
        var builder = new ShellBuilder()
            .RegisterCommand<EmptyCommand>("docker run", (_, _, _) => { })
            .RegisterCommand<EmptyCommand>(
                new TemplateBuilder()
                    .WithName("docker run")
                    .AddParameter(new TemplateParameterBuilder().WithName("-d")),
                (_, _, _) => { });
        
        var exception = Assert.Throws<ValidationException>(() => builder.Build())!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ValidationErrorCode.DuplicateInputTemplate));
    }
}