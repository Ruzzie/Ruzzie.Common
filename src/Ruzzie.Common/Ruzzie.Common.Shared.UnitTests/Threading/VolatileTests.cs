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

        class ValueReferenceType
        {
        }
    }
}
