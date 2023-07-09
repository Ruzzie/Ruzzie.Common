using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Numerics;

/// <summary>
/// Helper class for finding power of 2 numbers.
/// </summary>
public static class PowerOfTwoHelper
{
    /// <summary>
    /// Finds the nearest power of two equal or less than the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>integer that is a power of 2.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Cannot be negative.</exception>
    public static int FindNearestPowerOfTwoEqualOrLessThan(this int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot be negative.");
        }

        uint result = FindNearestPowerOfTwoEqualOrLessThan((uint)value);
        return (int)result;
    }

    /// <summary>
    /// Finds the nearest power of two equal or greater than the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>integer that is a power of 2.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Cannot be negative.
    /// or
    /// The value given would result in a value greater than 2^32 for a signed integer. Maximum value supported is  +
    ///                     maxSignedPowerOfTwo
    /// </exception>
    public static int FindNearestPowerOfTwoEqualOrGreaterThan(this int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot be negative.");
        }

        const int MAX_POWER_OF_TWO_VALUE_FOR_INT32 = 1 << 30; //1073741824;
        uint      result                           = PowTwoOf((uint)value);

        if (result > MAX_POWER_OF_TWO_VALUE_FOR_INT32)
        {
            throw new ArgumentOutOfRangeException(
                                                  nameof(value)
                                                , $"The value given would result in a value greater than 2^32 for a signed integer. Maximum value supported is {MAX_POWER_OF_TWO_VALUE_FOR_INT32}");
        }

        return (int)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint FindNearestPowerOfTwoEqualOrLessThan(this uint value)
    {
        if (value == 2)
        {
            return 2;
        }

        value = value >> 1;
        value++;

        return PowTwoOf(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PowTwoOf(uint value)
    {
        return BitOperations.RoundUpToPowerOf2(value);

        //uint x = value;
        /*x--; // comment out to always take the next biggest power of two, even if x is already a power of two
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        uint result = x + 1;
        return result;*/
    }

    /// <summary>
    /// Determines whether the value is a power of two.
    /// </summary>
    /// <param name="candidate">The x.</param>
    /// <returns>true if x is a power of two, otherwise false.</returns>
    [CLSCompliant(false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOfTwo(this uint candidate)
    {
        return (candidate & (candidate - 1)) == 0 && candidate != 0;
    }

    /// <summary>
    /// Determines whether the value is a power of two.
    /// </summary>
    /// <param name="candidate">The x.</param>
    /// <returns>true if x is a power of two, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOfTwo(this long candidate)
    {
        return (candidate & (candidate - 1)) == 0 && candidate > 0;
    }
}