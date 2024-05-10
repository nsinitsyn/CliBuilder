using System.Collections;

namespace ShellBuilder.Tests.Parsing;

public class SuccessfulParsingTestCases : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new object[]
        {
            "start [[Arg1]] [[Arg2]]",
            "start value1 value2",
            new List<(string Name, string Value)> { ("Arg1", "value1"), ("Arg2", "value2") }
        };
        
        yield return new object[]
        {
            "start [[Arg1]] [[Arg2]]",
            "start value1 123",
            new List<(string Name, string Value)> { ("Arg1", "value1"), ("Arg2", "123") }
        };
        
        yield return new object[]
        {
            "start operation [[Arg1]]",
            "start operation Value-1_123",
            new List<(string Name, string Value)> { ("Arg1", "Value-1_123") }
        };
        
        yield return new object[]
        {
            "start [[Arg1]] [[Arg2]]",
            "start \"value1\" \"value2\"",
            new List<(string Name, string Value)> { ("Arg1", "value1"), ("Arg2", "value2") }
        };
        
        yield return new object[]
        {
            "start [[Arg1]] [[Arg2]]",
            "start \"value 1\" \"Value-2\"",
            new List<(string Name, string Value)> { ("Arg1", "value 1"), ("Arg2", "Value-2") }
        };
        
        yield return new object[]
        {
            "start [[Site-Url1]]",
            "start \"SITE = http://site.com\"",
            new List<(string Name, string Value)> { ("Site-Url1", "SITE = http://site.com") }
        };

        yield return new object[]
        {
            "parser start [[Params]] [[Url]] [[ThreadsCount]] [[Args]]",
            "parser start \" __  param a b  c \" http://site1.com 4 \"xx = -189\"",
            new List<(string Name, string Value)>
            {
                ("Params", " __  param a b  c "),
                ("Url", "http://site1.com"),
                ("ThreadsCount", "4"),
                ("Args", "xx = -189")
            }
        };
    }
}