using MathCore;
using MathCore.Vectors;
using Task = System.Threading.Tasks.Task;

namespace Antennas;

public static class BeamPatternExtensions
{
    public class BeamAnalysandResult(
        FuncExtensions.SamplingResult<Complex>.Result[] Samples,
        int MaximumIndex,
        int Left07Index,
        int Right07Index,
        int LeftSLLIndex,
        int RightSLLIndex,
        double LeftAverageSLL,
        double RightAverageSLL,
        double Directivity,
        double Accuracy)
    {
        public int MaximumIndex { get; } = MaximumIndex;

        public double AngleOfMaximum => Samples[MaximumIndex].Argument;

        public Complex ValueOfMaxMaximum => Samples[MaximumIndex].Value;

        public (double, Complex) Maximum => Samples[MaximumIndex];

        public double BeamWidth => Samples[Right07Index].Argument - Samples[Left07Index].Argument;

        public double BeamCenterAngle => (Samples[Right07Index].Argument + Samples[Left07Index].Argument) / 2;

        public double Simetry
        {
            get
            {
                var result = (Maximum.Item1 - BeamCenterAngle) / BeamWidth;
                return result * result;
            }
        }

        public (double, Complex) LeftSideLobe => Samples[LeftSLLIndex];

        public (double, Complex) RightSideLobe => Samples[RightSLLIndex];

        public double LeftAverageSideLobLevel { get; } = LeftAverageSLL;

        public double RightAverageSideLobLevel { get; } = RightAverageSLL;

        public double Directivity { get; } = Directivity;

        public double Accuracy { get; } = Accuracy;
    }

    public static BeamAnalysandResult Analyze(
        this Func<double, Complex> f,
        double AngleMin = -Consts.pi, double AngleMax = Consts.pi,
        double Accuracy = 1e-2)
    {
        ArgumentNullException.ThrowIfNull(f);
        if (!double.IsFinite(Accuracy) || Accuracy <= 0)
            throw new ArgumentOutOfRangeException(nameof(Accuracy), "Точность должна быть положительным конечным числом");
        if (!double.IsFinite(AngleMin) || !double.IsFinite(AngleMax) || AngleMax <= AngleMin)
            throw new ArgumentException("Границы диапазона углов заданы неверно");

        var ff = f.SamplingAdaptive_OneWay(fv => fv.Power, AngleMin, AngleMax, Accuracy).ToArray();
        if (ff.Length == 0) throw new InvalidOperationException("Не удалось получить отсчёты диаграммы направленности");

        var max_pos = 0;
        for (var i = 1; i < ff.Length; i++)
            if (ff[i].Value.Power > ff[max_pos].Value.Power)
                max_pos = i;

        var max_power = ff[max_pos].Value.Power;
        var ff_2 = new double[ff.Length];
        for (var i = 0; i < ff.Length; i++)
            ff_2[i] = max_power > 0 ? ff[i].Value.Power / max_power : double.NaN;

        var index_07_left = FindHalfPowerIndex(ff_2, max_pos, -1);
        var index_07_right = FindHalfPowerIndex(ff_2, max_pos, +1);

        var max_angle = ff[max_pos].Argument;
        var beam_width = ff[index_07_right].Argument - ff[index_07_left].Argument;

        var left = CalculateSideLobe(ff_2, ff, 0, index_07_left - 1, max_angle - beam_width, -1);
        var right = CalculateSideLobe(ff_2, ff, index_07_right + 1, ff.Length - 1, max_angle + beam_width, +1);

        return new(
            ff, max_pos,
            index_07_left, index_07_right,
            left.Index, right.Index,
            left.Average, right.Average,
            GetDirectivity(ff),
            Accuracy
        );
    }

    private static int FindHalfPowerIndex(double[] normalized_power, int max_index, int direction)
    {
        const double half_power = 0.5;
        var index = max_index;
        while (index + direction >= 0 && index + direction < normalized_power.Length &&
               normalized_power[index + direction] > half_power)
            index += direction;
        return index;
    }

    private static (int Index, double Average) CalculateSideLobe(
        double[] normalized_power,
        FuncExtensions.SamplingResult<Complex>.Result[] samples,
        int start,
        int end,
        double angle_limit,
        int direction)
    {
        var fallback_index = Math.Clamp(direction < 0 ? start : end, 0, samples.Length - 1);
        if (start < 0 || end >= samples.Length || start > end) return (fallback_index, double.NaN);

        var index = direction < 0 ? end : start;
        while (index >= start && index <= end &&
               (direction < 0 ? samples[index].Argument > angle_limit : samples[index].Argument < angle_limit))
            index += direction;

        if (index < start || index > end) return (fallback_index, double.NaN);

        var max_value = double.NegativeInfinity;
        var max_index = index;
        var sum = 0d;
        var count = 0;
        for (var i = index; direction < 0 ? i >= start : i <= end; i += direction)
        {
            var value = normalized_power[i];
            if (double.IsNaN(value)) continue;
            sum += value;
            count++;
            if (value > max_value)
            {
                max_value = value;
                max_index = i;
            }
        }

        return (max_index, count > 0 ? sum / count : double.NaN);
    }

    private static double GetDirectivity(FuncExtensions.SamplingResult<Complex>.Result[] ff)
    {
        if (ff.Length < 2) return double.NaN;

        var first = 0;
        while (first < ff.Length && ff[first].Argument < -Consts.pi05) first++;
        var last = ff.Length - 1;
        while (last >= first && ff[last].Argument > Consts.pi05) last--;
        if (first >= ff.Length || last <= first) return double.NaN;

        var reference = first;
        for (var i = first + 1; i <= last; i++)
            if (ff[i].Value.Power > ff[reference].Value.Power)
                reference = i;

        var max_angle = ff[reference].Argument;
        var max_power = ff[reference].Value.Power;

        var integral = 0d;
        var last_v = ff[first].Value.Power;
        var last_a = ff[first].Argument;
        var last_cos = Math.Cos(last_a - max_angle);
        for (var i = first + 1; i <= last; i++)
        {
            var v = ff[i].Value.Power;
            var a = ff[i].Argument;
            var cos = Math.Cos(a - max_angle);
            integral += (v * cos + last_v * last_cos) / 2 * (a - last_a);
            last_v = v;
            last_a = a;
            last_cos = cos;
        }

        return integral > 0 ? max_power * 2 / integral : double.NaN;
    }

    public static Func<double, Complex> ToThetaPattern(Func<SpaceAngle, Complex> f, double Phi) => Theta => f(new(Theta, Phi));

    public static Func<double, Complex> ToPhiPattern(Func<SpaceAngle, Complex> f, double Theta) => Phi => f(new(Theta, Phi));

    public static double GetDirectivity(this Func<SpaceAngle, Complex> F, Action<double>? Complite = null) =>
        GetDirectivity((th, phi) => F(new(th, phi)), Complite);

    public static double GetDirectivity(this Func<double, double, Complex> F, Action<double>? Complite = null)
    {
        var i_p = 0;
        var N_p = 360;
        var N_t = 360;

        var f = Complite != null
            ? (Func<double, double>)(p =>
            {
                i_p++;
                var I1 = FuncExtensions.GetIntegralValue(t =>
                {
                    var I0 = F(t, p).Power * Math.Sin(t);
                    return I0;
                }, 0, Consts.pi, Consts.pi / N_t);
                Complite.Invoke((double)i_p / (N_p + 1));
                return I1;
            })
            : p => FuncExtensions.GetIntegralValue(t => F(t, p).Power * Math.Sin(t), 0, Consts.pi, Consts.pi / N_t);

        return 2 * Consts.pi2 / f.GetIntegralValue(0, Consts.pi2, Consts.pi2 / N_p);
    }

    public static double GetDirectivityBuffered(this Func<SpaceAngle, Complex> F, Action<double>? Complite = null) =>
        GetDirectivityBuffered((th, phi) => F(new(th, phi)), Complite);

    public static double GetDirectivityBuffered(this Func<double, double, Complex> F, Action<double>? Complite = null)
    {
        var i_p    = 0;
        var N_p    = 360;
        var N_t    = 360;
        var buffer = new Dictionary<double, Dictionary<double, double>>(360);

        var f = Complite != null
            ? (Func<double, double>)(p =>
            {
                var b = buffer.GetValueOrAddNew(p, () => new(360));
                i_p++;
                Func<double, double> f0 = t => b.GetValueOrAddNew(t, th => F(th, p).Power * Math.Sin(th));
                var                  I1 = f0.GetIntegralValue(0, Consts.pi, Consts.pi / N_t);
                Complite.Invoke((double)i_p / (N_p + 1));
                return I1;
            })
            : p =>
            {
                var                  b  = buffer.GetValueOrAddNew(p, () => new(360));
                Func<double, double> f0 = t => b.GetValueOrAddNew(t, th => F(th, p).Power * Math.Sin(th));
                return f0.GetIntegralValue(0, Consts.pi, Consts.pi / N_t);
            };

        return 2 * Consts.pi2 / f.GetIntegralValue(0, Consts.pi2, Consts.pi2 / N_p);
    }

    public static double GetDirectivity(this Func<double, Complex> Pattern)
    {
        var buffer = new Dictionary<double, Complex>();
        Complex F(double A) => buffer.GetValueOrAddNew(A, Pattern);

        return 2 / FuncExtensions.GetIntegralValue_Adaptive(th => F(th).Power * Math.Cos(th), -Consts.pi, Consts.pi);
    }

    public static Task<double> GetDirectivityAsync(this Func<double, Complex> F) => Task.Run(F.GetDirectivity);

    public static double GetDirectivityBuffered(this Func<double, Complex> F) => 2 / FuncExtensions.GetIntegralValue_Adaptive(th => F(th).Power * Math.Cos(th), -Consts.pi, Consts.pi);

    public static Task<double> GetDirectivityBufferedAsync(this Func<double, Complex> F) => Task.Run(F.GetDirectivityBuffered);
}