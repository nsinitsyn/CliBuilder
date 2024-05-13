using CliBuilderCore.Command;
using CliBuilderCore.Command.Templates;
using CliBuilderCore.Parsing;

namespace CliBuilderTests.Parsing.ParameterizedTemplate;

public class ParameterizedTemplateParserTests
{
    [Test]
    public void ParsingSuccessful_WithCommand1()
    {
        var template = new CliBuilderCore.Command.Templates.ParameterizedTemplate(
            "run",
            [
                new TemplateParameter("-arg1", null, true, false, null, "[[Arg1]]", null),
                new TemplateParameter("-arg2", null, true, false, null, "[[Arg2]]", null),
                new TemplateParameter("-arg3", null, false, false, null, "[[Arg3]]", null)
            ]);

        var input = "run -arg1 144,5 -arg2 \"This is argument 2!\"";

        var cmd = new CliCommandWithHandler<Command1>(typeof(Command1), template, (_, _, _) => { }, null);
        
        var parsingResult = ParameterizedTemplateParser.TryParse(input, template, cmd);
        
        Assert.That(parsingResult.Parsed, Is.True);

        Assert.Multiple(() =>
        {
            var command = (Command1)parsingResult.CommandInstance!;
            
            Assert.That(command.Arg1, Is.EqualTo(144.5));
            Assert.That(command.Arg2, Is.EqualTo("This is argument 2!"));
        });
    }
    
    [Test]
    public void ParsingSuccessful_WithCommand2()
    {
        var template = new CliBuilderCore.Command.Templates.ParameterizedTemplate(
            "run",
            [
                new TemplateParameter("-arg1", null, true, false, null, "[[Arg1]]", null),
                new TemplateParameter("-arg2", null, false, false, null, "[[Arg2]]", null),
                new TemplateParameter("-arg3", null, true, true, null, "[[Arg3]]", null),
                new TemplateParameter("-arg4", null, true, false, null, null, "Arg4"),
                new TemplateParameter("-arg5", "--argument5", true, false, "Arg5", "[[Var1]],[[Var2]]:[[Var3]]", null),
                new TemplateParameter("-arg6", null, false, true, "Arg6", "[[ItemA]]=[[ItemB]]", null)
            ]);

        var input = "run -arg1 144,5 -arg2 \"This is argument 2.\" -arg6 1993=19 -arg3 Text-1 -arg3 Text_2 -arg4 --argument5 \"190,Some text:another_text\" -arg6 144=67,01";

        var cmd = new CliCommandWithHandler<Command2>(typeof(Command2), template, (_, _, _) => { }, null);
        
        var parsingResult = ParameterizedTemplateParser.TryParse(input, template, cmd);
        
        Assert.That(parsingResult.Parsed, Is.True);

        Assert.Multiple(() =>
        {
            var command = (Command2)parsingResult.CommandInstance!;
            
            Assert.That(command.Arg1, Is.EqualTo(144.5));
            
            Assert.That(command.Arg2, Is.EqualTo("This is argument 2."));
            
            Assert.That(command.Arg3, Has.Count.EqualTo(2));
            Assert.That(command.Arg3![0], Is.EqualTo("Text-1"));
            Assert.That(command.Arg3[1], Is.EqualTo("Text_2"));
            
            Assert.That(command.Arg4, Is.True);
            
            Assert.That(command.Arg5!.Var1, Is.EqualTo(190));
            Assert.That(command.Arg5.Var2, Is.EqualTo("Some text"));
            Assert.That(command.Arg5.Var3, Is.EqualTo("another_text"));
            
            Assert.That(command.Arg6, Has.Count.EqualTo(2));
            Assert.That(command.Arg6![0].ItemA, Is.EqualTo(1993));
            Assert.That(command.Arg6[0].ItemB, Is.EqualTo(19));
            Assert.That(command.Arg6![1].ItemA, Is.EqualTo(144));
            Assert.That(command.Arg6[1].ItemB, Is.EqualTo(67.01));
        });
    }
    
    [Test]
    public void ParsingFailed_ParameterNotFound()
    {
        var template = new CliBuilderCore.Command.Templates.ParameterizedTemplate(
            "run",
            [
                new TemplateParameter("-arg1", null, true, false, null, "[[Arg1]]", null),
                new TemplateParameter("-arg2", null, true, false, null, "[[Arg2]]", null),
                new TemplateParameter("-arg3", null, true, false, null, "[[Arg3]]", null)
            ]);

        var input = "run -arg1 144,5 -arg4 100";

        var cmd = new CliCommandWithHandler<Command1>(typeof(Command1), template, (_, _, _) => { }, null);
        
        var parsingResult = ParameterizedTemplateParser.TryParse(input, template, cmd);
        
        Assert.That(parsingResult.Parsed, Is.False);
        Assert.That(parsingResult.NotFoundParameterName, Is.EqualTo("-arg4"));
    }
    
    [Test]
    public void ParsingFailed_DuplicateParameter()
    {
        var template = new CliBuilderCore.Command.Templates.ParameterizedTemplate(
            "run",
            [
                new TemplateParameter("-arg1", null, true, false, null, "[[Arg1]]", null),
                new TemplateParameter("-arg2", null, true, false, null, "[[Arg2]]", null),
                new TemplateParameter("-arg3", null, true, false, null, "[[Arg3]]", null)
            ]);

        var input = "run -arg1 144,5 -arg2 Text -arg1 100";

        var cmd = new CliCommandWithHandler<Command1>(typeof(Command1), template, (_, _, _) => { }, null);
        
        var parsingResult = ParameterizedTemplateParser.TryParse(input, template, cmd);
        
        Assert.That(parsingResult.Parsed, Is.False);
        Assert.That(parsingResult.DuplicateParameterName, Is.EqualTo("-arg1"));
    }
    
    [Test]
    public void ParsingFailed_MissingRequiredParameters()
    {
        var template = new CliBuilderCore.Command.Templates.ParameterizedTemplate(
            "run",
            [
                new TemplateParameter("-arg1", null, true, false, null, "[[Arg1]]", null),
                new TemplateParameter("-arg2", null, true, false, null, "[[Arg2]]", null),
                new TemplateParameter("-arg3", null, true, false, null, "[[Arg3]]", null)
            ]);

        var input = "run -arg2 \"This is argument 2.\"";

        var cmd = new CliCommandWithHandler<Command1>(typeof(Command1), template, (_, _, _) => { }, null);
        
        var parsingResult = ParameterizedTemplateParser.TryParse(input, template, cmd);
        
        Assert.That(parsingResult.Parsed, Is.False);

        Assert.Multiple(() =>
        {
            Assert.That(parsingResult.MissingRequiredParameterNames, Has.Count.EqualTo(2));
            Assert.That(parsingResult.MissingRequiredParameterNames![0], Is.EqualTo("-arg1"));
            Assert.That(parsingResult.MissingRequiredParameterNames[1], Is.EqualTo("-arg3"));
        });
    }
}