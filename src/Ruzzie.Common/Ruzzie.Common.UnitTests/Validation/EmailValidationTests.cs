using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Ruzzie.Common.Validation.UnitTests
{
    public class EmailValidationTests
    {
        [InlineData("mail",                     false)]
        [InlineData("j..s@@test.com",           false)]
        [InlineData("js@test..com",             false)]
        [InlineData("js@acme.中国",               true)]
        [InlineData("ruzzie+jace@acme.com",     true)]
        [InlineData("ruzzie.jace@acme.com",     true)]
        [InlineData("ruzzie@acme-main.com",     true)]
        [InlineData("ruzzie1337@acme.com",      true)]
        [InlineData("ruzzie.mid.jace@acme.com", true)]
        [InlineData("email@[123.123.123.123]",  true)]
        [InlineData("\"email\"@example.com",    true)]
        //[InlineData("very.unusual.”@”.unusual.com@example.com",true)]
        [InlineData("much.\"more \\\\ unusual\"@example.com", true)]
        [Theory]
        public void SmokeTest(string email, bool expected)
        {
            email.IsValidEmailAddress().Should().Be(expected);
        }

        [Property]
        public void NoExceptionShouldBeThrownPropertyTest(string email)
        {
            email.IsValidEmailAddress();
        }
    }
}