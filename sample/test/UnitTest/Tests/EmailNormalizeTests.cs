using Application.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Tests;

public class EmailNormalizeTests : AppFactory
{
    public EmailNormalizeTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [InlineData("t.test@gmail.com", "ttest@gmail.com")]
    public void EmailNormalize(string input, string expectedOutput)
    {
        var result = input.EmailNormalize();
        OutputHelper.WriteLine(result);
        Assert.Equal(expectedOutput, result);
    }
}