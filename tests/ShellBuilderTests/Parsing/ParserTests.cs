using ShellBuilderCore;

namespace ShellBuilderTests.Parsing;

public class ParserTests
{
    [TestCaseSource(typeof(SuccessfulParsingTestCases))]
    public void SuccessfulParsingTest(string template, string input, List<(string Name, string Value)> expectedItems)
    {
        var parsed = Parser.TryParse(input, template, out var items);
        
        Assert.That(parsed, Is.EqualTo(true));
        CollectionAssert.AreEqual(expectedItems, items);
    }
    
    [TestCaseSource(typeof(FailedParsingTestCases))]
    public void FailedParsingTest(string template, string input)
    {
        var parsed = Parser.TryParse(input, template, out _);
        Assert.That(parsed, Is.EqualTo(false));
    }
}