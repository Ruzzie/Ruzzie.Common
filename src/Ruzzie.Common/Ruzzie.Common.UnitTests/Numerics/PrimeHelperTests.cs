using System;
using NUnit.Framework;
using Ruzzie.Common.Numerics;

namespace Ruzzie.Common.UnitTests.Numerics
{
    [TestFixture]
    public class PrimeHelperTests
    {
        [TestCase(0,false)]
        [TestCase(1,false)]
        [TestCase(2,true)]
        [TestCase(709, true)]
        [TestCase(7199369, true)]
        [TestCase(7199368, false)]
        [TestCase(9199361, false)]
        public void IsPrime(int number, bool expected)
        {
            Assert.That(PrimeHelper.IsPrime(number),Is.EqualTo(expected));
        }

        [TestCase(0,3)]
        [TestCase(1,3)]
        [TestCase(2,3)]
        [TestCase(700,761)]
        [TestCase(123123, 130363)]
        [TestCase(7199368, 7199369)]
        [TestCase(9199361, 9199391)]
        [TestCase(9199391, 9199391)]
        [TestCase(Int64.MaxValue,3)]
        [TestCase(Int64.MaxValue-2,3)]
        public void GetPrime(long min, long expected)
        {
            Assert.That(min.GetPrime(),Is.EqualTo(expected));
        }     
    }
}
