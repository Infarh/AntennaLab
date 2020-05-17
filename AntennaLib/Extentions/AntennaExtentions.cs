using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore;

// ReSharper disable once CheckNamespace
namespace Antennas
{
    public static class AntennaExtensions
    {
        private static IEnumerable<double> GetAngles(double th1, double th2, double dth)
        {
            var th = Math.Min(th1, th2);
            th2 = Math.Max(th1, th2);
            dth = Math.Abs(dth);
            do
            {
                yield return th;
            } while ((th += dth) < th2);
            yield return th2;
        }

        public static PatternValue[] GetPatternPhi
        (
            this Antenna antenna,
            double f,
            double phi = 0 * Consts.ToRad,
            double th1 = -180 * Consts.ToRad,
            double th2 = 180 * Consts.ToRad,
            double dth = 1 * Consts.ToRad
        )
        {
            var th = Math.Min(th1, th2);
            dth = Math.Abs(dth);
            var result = new PatternValue[(int)((Math.Max(th1, th2) - Math.Min(th1, th2)) / dth) + 1];
            for (var i = 0; i < result.Length; i++, th += dth)
                result[i] = new PatternValue(th, antenna.Pattern(th, phi, f));
            return result;
        }

        public static PatternValue[] GetPatternValuesParallel
        (  
            this Antenna antenna,
            double f,
            double phi = 0 * Consts.ToRad,
            double th1 = -181 * Consts.ToRad,
            double th2 = 181 * Consts.ToRad,
            double dth = 1 * Consts.ToRad,
            CancellationToken Cancel = default
        )
        {
            var parallel_query = GetAngles(th1, th2, dth).AsParallel().AsOrdered();
            if (Cancel != default)
                parallel_query = parallel_query.WithCancellation(Cancel);
            return parallel_query.Select(th => new PatternValue(th, antenna.Pattern(th, phi, f))).ToArray();
        }

        public static Task<PatternValue[]> GetPatternPhiAsync
        (
            this Antenna antenna,
            double f,
            double phi = 0 * Consts.ToRad,
            double th1 = -180 * Consts.ToRad,
            double th2 = 180 * Consts.ToRad,
            double dth = 1 * Consts.ToRad,
            IProgress<PatternCalculationTaskProgressInfo> Progress = null,
            CancellationToken Cancel = default
        ) => Task.Factory.StartNew(() =>
        {                 
            var th = Math.Min(th1, th2);
            dth = Math.Abs(dth);
            var result = new PatternValue[(int)((Math.Max(th1, th2) - Math.Min(th1, th2)) / dth) + 1];
            for (int i = 0, len = result.Length; i < len && !Cancel.IsCancellationRequested; i++, th += dth)
            {
                var pattern_value = new PatternValue(th, antenna.Pattern(th, phi, f));
                result[i] = pattern_value;
                Progress?.Report(new PatternCalculationTaskProgressInfo((double)i / len, pattern_value));
            }
            Cancel.ThrowIfCancellationRequested();
            return result;
        }, Cancel);
    }
}
