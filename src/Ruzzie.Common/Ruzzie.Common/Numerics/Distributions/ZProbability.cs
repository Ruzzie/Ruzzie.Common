using System;

namespace Ruzzie.Common.Numerics.Distributions;

/// <summary>
/// Methods for z distribution
/// </summary>
public static class ZProbability
{
    #region adapted from entlib z.c

    /*HEADER
        Module:       z.c
        Purpose:      compute approximations to normal z distribution probabilities
        Programmer:   Gary Perlman
        Organization: Wang Institute, Tyngsboro, MA 01879
        Copyright:    none
    */

    /// <summary>
    /// The maximum meaningful z value. 6.0.
    /// </summary>
    private const double ZMax = 6.0;

    /// <summary>
    /// probability of normal z value
    /// </summary>
    /// <param name="normalZValue">normal z value</param>
    /// <returns>returns cumulative probability from -oo to z</returns>
    /// <remarks>This routine has six digit accuracy, so it is only useful for z values &lt; 6.  For z values &gt;= to 6.0, poz() returns 0.0.</remarks>
    /// <remarks>
    /// Adapted from a polynomial approximation in:
    /// Ibbetson D, Algorithm 209
    /// Collected Algorithms of the CACM 1963 p. 616
    /// </remarks>
    public static double ProbabilityOfZ(in double normalZValue)
    {
        double y, x, w;

        if (Math.Abs(normalZValue) < double.Epsilon)
        {
            x = 0.0;
        }
        else {
            y = 0.5 * Math.Abs(normalZValue);
            if (y >= (ZMax * 0.5))
            {
                x = 1.0;
            }
            else if (y < 1.0)
            {
                w = y * y;
                x = ((((((((0.000124818987 * w
                            - 0.001075204047) * w + 0.005198775019) * w
                          - 0.019198292004) * w + 0.059054035642) * w
                        - 0.151968751364) * w + 0.319152932694) * w
                      - 0.531923007300) * w + 0.797884560593) * y * 2.0;
            }
            else {
                y -= 2.0;
                x = (((((((((((((-0.000045255659 * y
                                 + 0.000152529290) * y - 0.000019538132) * y
                               - 0.000676904986) * y + 0.001390604284) * y
                             - 0.000794620820) * y - 0.002034254874) * y
                           + 0.006549791214) * y - 0.010557625006) * y
                         + 0.011630447319) * y - 0.009279453341) * y
                       + 0.005353579108) * y - 0.002141268741) * y
                     + 0.000535310849) * y + 0.999936657524;
            }
        }
        return (normalZValue > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5));
    }
    #endregion
}