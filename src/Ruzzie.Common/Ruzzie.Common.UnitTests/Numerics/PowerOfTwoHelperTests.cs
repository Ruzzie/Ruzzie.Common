﻿using System;
using NUnit.Framework;
using Ruzzie.Common.Numerics;

namespace Ruzzie.Common.UnitTests.Numerics
{
    [TestFixture]
    public class PowerOfTwoHelperTests
    {

        [TestCase(2, 2)]
        [TestCase(250, 256)]
        [TestCase(100, 128)]
        [TestCase(1000, 1024)]
        [TestCase(1024, 1024)]
        [TestCase(1500, 2048)]
        [TestCase(60000, 65536)]
        [TestCase(100000, 131072)]
        [TestCase(1048570, 1048576)]
        [TestCase(4194000, 4194304)]
        [TestCase(1073741800, 1073741824)]
        public void FindNearestPowerOfTwoForGivenValue(int value, int expected)
        {
            Assert.That(value.FindNearestPowerOfTwoEqualOrGreaterThan(), Is.EqualTo(expected));
        }

        [TestCase(2, 2)]
        [TestCase(5, 4)]
        [TestCase(300, 256)]
        [TestCase(140, 128)]
        [TestCase(1050, 1024)]
        [TestCase(1024, 1024)]
        [TestCase(3000, 2048)]
        [TestCase(70000, 65536)]
        [TestCase(140000, 131072)]
        [TestCase(1148570, 1048576)]
        [TestCase(4494000, 4194304)]
        [TestCase(1273741800, 1073741824)]
        public void FindNearestPowerOfTwoLessThanForGivenValue(int value, int expected)
        {
            Assert.That(value.FindNearestPowerOfTwoEqualOrLessThan(), Is.EqualTo(expected));
        }

        [Test]
        public void FindNearestPowerOfTwoThrowsArgumentExceptionWhenTargetValueWouldBegreaterThanMaxInt32()
        {
            Assert.That(() => (int.MaxValue - 1).FindNearestPowerOfTwoEqualOrGreaterThan(), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void FindNearestPowerOfTwoThrowsArgumentExceptionWhenTargetValueIsLessThan0()
        {
            Assert.That(() => (-100).FindNearestPowerOfTwoEqualOrGreaterThan(), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void FindNearestPowerOfTwoEqualOrLessThanThrowsArgumentExceptionWhenValueIsLessThanOrEqualZero(int value)
        {
            Assert.That(()=>value.FindNearestPowerOfTwoEqualOrLessThan(), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(2,true)]
        [TestCase(3, false)]
        [TestCase(1024, true)]
        [TestCase(999, false)]
        [TestCase(1073741824, true)]
        [TestCase(2073741824, false)]
        public void IsPowerOfTwoTests(long value, bool expected)
        {
            Assert.That(value.IsPowerOfTwo(), Is.EqualTo(expected));
        }
    }
}