using CliBuilderCore.Parsing;

namespace CliBuilderTests.Parsing.StringTemplate;

public class StringTemplateParserTests
{
    [TestCaseSource(typeof(SuccessfulParsingTestCases))]
    public void SuccessfulParsingTest(string template, string input, List<(string Name, string Value)> expectedItems)
    {
        var parsed = StringTemplateParser.TryParse(input, template, out var items);
        
        Assert.That(parsed, Is.EqualTo(true));
        CollectionAssert.AreEqual(expectedItems, items);
    }
    
    [TestCaseSource(typeof(FailedParsingTestCases))]
    public void FailedParsingTest(string template, string input)
    {
        var parsed = StringTemplateParser.TryParse(input, template, out _);
        Assert.That(parsed, Is.EqualTo(false));
    }
}