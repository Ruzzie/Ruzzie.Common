using FluentAssertions;
using Ruzzie.Common.Threading;
using Xunit;

namespace Ruzzie.Common.UnitTests.Threading;

public class VolatileTests
{
    [Fact]
    public void ReadApiSmokeTest()
    {
        //Arrange
        ValueReferenceType value = new ValueReferenceType();
        //Act
        var read = Volatile.Read(ref value);
        //Act
        value.Should().Be(read);
    }

    [Fact]
    public void WriteApiSmokeTest()
    {
        //Arrange
        ValueReferenceType value        = new ValueReferenceType();
        ValueReferenceType valueToWrite = new ValueReferenceType();
        //Act
        Volatile.Write(ref value, valueToWrite);
        //Assert
        value.Should().Be(valueToWrite);
    }

    [Fact]
    public void ReadValueTypeApiSmokeTest()
    {
        //Arrange
        ValueType value = new ValueType();
        //Act
        var read = Volatile.ReadValueType(ref value);
        //Act
        value.Should().Be(read);
    }

    [Fact]
    public void WriteValueTypeApiSmokeTest()
    {
        //Arrange
        ValueType value        = new ValueType();
        ValueType valueToWrite = new ValueType();
        //Act
        Volatile.WriteValueType(ref value, valueToWrite);
        //Assert
        value.Should().Be(valueToWrite);
    }

    class ValueReferenceType
    {
    }

    struct ValueType
    {
            
    }
}