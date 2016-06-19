using NUnit.Framework;
using Ruzzie.Common.Threading;

namespace Ruzzie.Common.Shared.UnitTests.Threading
{
    [TestFixture]
    public class VolatileTests
    {
        [Test]
        public void ReadApiSmokeTest()
        {
            //Arrange
            ValueReferenceType value = new ValueReferenceType();
            //Act
            var read = Volatile.Read(ref value);
            //Act
            Assert.That(value, Is.EqualTo(read));
        }

        [Test]
        public void WriteApiSmokeTest()
        {
            //Arrange
            ValueReferenceType value = new ValueReferenceType();
            ValueReferenceType valueToWrite = new ValueReferenceType();
            //Act
            Volatile.Write(ref value,valueToWrite);
            //Assert
            Assert.That(value, Is.EqualTo(valueToWrite));
        }

        [Test]
        public void ReadValueTypeApiSmokeTest()
        {
            //Arrange
            ValueType value = new ValueType();
            //Act
            var read = Volatile.ReadValueType(ref value);
            //Act
            Assert.That(value, Is.EqualTo(read));
        }

        [Test]
        public void WriteValueTypeApiSmokeTest()
        {
            //Arrange
            ValueType value = new ValueType();
            ValueType valueToWrite = new ValueType();
            //Act
            Volatile.WriteValueType(ref value, valueToWrite);
            //Assert
            Assert.That(value, Is.EqualTo(valueToWrite));
        }

        class ValueReferenceType
        {
        }

        struct ValueType
        {
            
        }
    }
}
