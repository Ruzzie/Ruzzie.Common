using System.Diagnostics;
using FluentAssertions;
using Ruzzie.Common.Hashing;
using Xunit;
using Xunit.Abstractions;

namespace Ruzzie.Common.UnitTests.Hashing;

public class HashStringCaseInsensitiveTests
{
    private readonly ITestOutputHelper  _testOutputHelper;
    private readonly FNV1AHashAlgorithm _hashAlgorithm = new FNV1AHashAlgorithm();

    public HashStringCaseInsensitiveTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("The Doctor",       "the doctor")]
    [InlineData("the Doctor",       "the doctor")]
    [InlineData("A",                "a")]
    [InlineData("Ab",               "aB")]
    [InlineData("AB",               "AB")]
    [InlineData("1!!",              "1!!")]
    [InlineData("Ω",                "ω")]
    [InlineData("3 Harvard Square", "3 HARVARD SQUARE")]
    public void IgnoreCaseTests(string casingOne, string casingStyleTwo)
    {
        _hashAlgorithm.HashStringCaseInsensitive(casingOne)
                      .Should()
                      .Be(_hashAlgorithm.HashStringCaseInsensitive(casingStyleTwo));
    }

    [Fact]
    public void NullShouldNotThrowException()
    {
        _hashAlgorithm.HashStringCaseInsensitive(null).Should().Be(-2128831035);
    }

    [Fact]
    public void EmptyShouldReturnDefaultHashValue()
    {
        _hashAlgorithm.HashStringCaseInsensitive("").Should().Be(-2128831035);
    }

    [Fact]
    public void PerformanceTest()
    {
        SimpleRandom random             = new SimpleRandom(7 * 37);
        int          numberOfIterations = 100000;
        //Create random strings
        var randomStrings = new string[numberOfIterations];
        for (int i = 0; i < numberOfIterations; i++)
        {
            //create string of random length
            int    strLength = random.Next(10, 100);
            char[] chars     = new char[strLength];
            for (int j = 0; j < strLength; j++)
            {
                chars[j] = (char)random.Next(1, ushort.MaxValue + 1);
            }

            randomStrings[i] = new string(chars);
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < numberOfIterations; i++)
        {
            _hashAlgorithm.HashStringCaseInsensitive(randomStrings[i]);
        }

        sw.Stop();

        _testOutputHelper.WriteLine("For "                  + numberOfIterations + " iterations. Total time of:" +
                                    sw.Elapsed.TotalSeconds + " seconds. ticks / string: " +
                                    (sw.Elapsed.Ticks / numberOfIterations));
    }
}