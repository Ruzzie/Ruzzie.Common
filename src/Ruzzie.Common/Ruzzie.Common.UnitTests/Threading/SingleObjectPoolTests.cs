using FluentAssertions;
using Ruzzie.Common.Threading;
using Xunit;

namespace Ruzzie.Common.UnitTests.Threading;

public class SingleObjectPoolTests
{
    [Fact]
    public void SmokeTest()
    {
        //Arrange
        using (var singlePool = new SingleObjectPool<string>("Test"))
        {
            //Act
            var result = singlePool.ExecuteOnAvailableObject(s => s.Contains("t"));
            //Assert
            result.Should().BeTrue();
        }
    }
}