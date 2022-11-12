#nullable enable
using Antennas;
using MathCore;

namespace ConsolePolygon;

internal static class BeamPatternAnalisysTest
{
    public static void Test()
    {
        const double f0     = 1e9;
        const double lambda = Consts.SpeedOfLight / f0;
        const double k      = Consts.pi2 / lambda;
        const double d      = lambda / 2;
        const int    N      = 16;

        var array = new LinearAntennaArray(N, d);

        var f = array.GetPatternOfThetaOnFreq(f0);

        var result = f.Analyze();
    }
}