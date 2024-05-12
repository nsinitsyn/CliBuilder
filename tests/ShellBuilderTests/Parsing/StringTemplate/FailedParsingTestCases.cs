using System.Collections;

namespace ShellBuilderTests.Parsing.StringTemplate;

public class FailedParsingTestCases : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new object[]
        {
            "start [[Arg1]] [[Arg2]]",
            "start value1 value2 value3"
        };

        yield return new object[]
        {
            "start [[Arg1]] [[Arg2]] [[Arg3]]",
            "start value1 value2"
        };
        
        yield return new object[]
        {
            "start [[Arg1]] [[Arg2]]",
            "run value1 value2"
        };
        
        yield return new object[]
        {
            "start [[Url]]",
            "start SITE = http://site.com"
        };
    }
}