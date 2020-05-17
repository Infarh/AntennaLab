using System;
using Antennas;
using MathCore;

namespace ConsolePolygon
{
    internal static class BeamPatternExpression
    {
        public static void Test()
        {
            const double f0 = 1e9;
            const double lambda = Consts.SpeedOfLight / f0;
            var array = AntennaArray.CreateLinearArray(new Antenna[4].Initialize(i => new UniformAntenna()), lambda/2);

            var EF = array.GetPatternExpression();
            var F = EF.Compile();
        }
    }
}