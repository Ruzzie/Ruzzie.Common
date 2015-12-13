﻿using NUnit.Framework;

namespace Ruzzie.Common.Numerics.UnitTests
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

        [TestCase(1,3)]
        [TestCase(700,761)]
        [TestCase(123123, 130363)]
        [TestCase(7199368, 7199369)]
        [TestCase(9199361, 9199391)]
        [TestCase(9199391, 9199391)]
        public void GetPrime(int min, int expected)
        {
            Assert.That(PrimeHelper.GetPrime(min),Is.EqualTo(expected));
        }     
    }
}
