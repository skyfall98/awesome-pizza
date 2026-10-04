using AwesomePizza.Api.Service;

namespace AwesomePizza.Tests.Unit;

public class OrderCodeGeneratorTests
{
    // The code is 6 characters long, uppercase letters or digits, without the ambiguous 0, O, 1 and I.
    [Fact]
    public void Generate_ReturnsSixCharactersWithoutAmbiguousOnes()
    {
        for (int i = 0; i < 500; i++)
        {
            string code = OrderCodeGenerator.Generate();

            Assert.Equal(6, code.Length);
            Assert.All(code, c =>
            {
                Assert.True(char.IsAsciiLetterUpper(c) || char.IsAsciiDigit(c));
                Assert.DoesNotContain(c, "01OI");
            });
        }
    }
}
