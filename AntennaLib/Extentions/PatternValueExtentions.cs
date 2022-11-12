#nullable enable
using MathCore;

// ReSharper disable once CheckNamespace
namespace Antennas;

public static class PatternValueExtensions
{
    public static PatternValue GetGain(this IEnumerable<PatternValue> pattern) => pattern.GetMax(p => p.Value.Abs);

    public static AnalysePatternResult Analyse(this PatternValue[] pattern)
    {
        pattern.Analyse
        (
            out var max, out var max_index, out var max_angle, out var d0,
            out var th07_left_index, out var th07_right_index,
            out var th07_left, out var th07_right,
            out var th0_left_index, out var th0_right_index,
            out var th0_left, out var th0_right,
            out var left_ssl, out var left_sll_index,
            out var right_sll, out var right_sll_index,
            out var ssl, out var left_mean_sll, out var right_mean_sll, out var mean_sll
        );
        return new AnalysePatternResult(max, max_index, max_angle, d0,
            th07_left_index, th07_right_index, th07_left, th07_right,
            th0_left_index, th0_right_index, th0_left, th0_right,
            left_ssl, left_sll_index, right_sll, right_sll_index,
            ssl, left_mean_sll, right_mean_sll, mean_sll);
    }

    /// <summary>Анализ диаграммы направленности</summary>
    /// <param name="pattern">Массив отсчётов ДН</param>
    /// <param name="max">Максимальное значение ДН</param>
    /// <param name="max_index">Индекс максимального значения ДН</param>
    /// <param name="D0">КНД</param>
    /// <param name="th07_left_index">Индекс левой границы луча по уровню 0.707 <paramref name="max"/></param>
    /// <param name="th07_right_index">Индекс правой границы луча по уровню 0.707 <paramref name="max"/></param>
    /// <param name="th07_left">Угловое положение левой границы луча по уровню 0.707 <paramref name="max"/></param>
    /// <param name="th07_right">Угловое положение правой границы луча по уровню 0.707 <paramref name="max"/></param>
    /// <param name="th0_left_index">Индекс левой границы луча по "нулю" ДН</param>
    /// <param name="th0_right_index">Индекс правой границы луча по "нулю" ДН</param>
    /// <param name="LeftSLL">УБЛ левой части ДН</param>
    /// <param name="LeftSLLIndex">Индекс максимального боковоего лепестка левой части ДН</param>
    /// <param name="RightSLL">УБЛ правой части ДН</param>
    /// <param name="RightSLLIndex">Индекс максимального боковоего лепестка правой части ДН</param>
    /// <param name="SLL">Общий УБЛ ДН</param>
    /// <param name="LeftMeanSLL">Средний УБЛ левой части ДН</param>
    /// <param name="RightMeanSLL">Средний УБЛ правой части ДН</param>
    /// <param name="MeanSLL">Средний УБЛ</param>
    public static void Analyse
    (
        this PatternValue[] pattern,
        out double max, out int max_index, out double max_angle, out double D0,
        out int th07_left_index, out int th07_right_index,
        out double th07_left, out double th07_right,
        out int th0_left_index, out int th0_right_index,
        out double th0_left, out double th0_right,
        out double LeftSLL,
        out int LeftSLLIndex,
        out double RightSLL,
        out int RightSLLIndex,
        out double SLL,
        out double LeftMeanSLL,
        out double RightMeanSLL,
        out double MeanSLL
    )
    {
        if (pattern is null) throw new ArgumentNullException(nameof(pattern));
        if (pattern.Length == 0) throw new InvalidOperationException("Диагамма не содержит значений");
        var len = pattern.Length;
        //var center = pattern.Where(v => v.Angle >= Consts.pi05neg && v.Angle <= Consts.pi05).ToArray();
        //var pattern_max = center.GetMax(v => v.Value.Abs, out max_index);
        //max = pattern_max.Value.Abs;
        max       = double.NegativeInfinity;
        max_index = -1;
        max_angle = double.NaN;
        var th_pi05_index = 0;
        for (var i = 0; i < len && pattern[i].Angle <= Consts.pi05; i++)
            if (pattern[i].Angle >= Consts.pi05neg)
            {
                var v = pattern[i].Value.Abs;
                if (v <= max) continue;
                max       = v;
                max_index = i;
                max_angle = pattern[i].Angle;
            }
            else
                th_pi05_index = i;

        var last_v     = pattern[th_pi05_index].Value.Power;
        var last_a     = pattern[th_pi05_index].Angle;
        var last_cos_a = Math.Cos(last_a);
        var integral   = 0d;
        for (var i = th_pi05_index + 1; i < len && pattern[i].Angle <= Consts.pi05; i++)
        {
            var v     = pattern[i].Value.Power;
            var a     = pattern[i].Angle;
            var cos_a = Math.Cos(a - max_angle);
            integral   += (v * cos_a + last_v * last_cos_a) / 2 * (a - last_a);
            last_v     =  v;
            last_a     =  a;
            last_cos_a =  cos_a;
        }
        D0 = max * max * 2 / integral;

        pattern.FindBeamWidth07(len, max_index, max, out th07_left_index, out th07_right_index, out th07_left, out th07_right);

        pattern.FindBeamWidth0(len, max, th07_left_index, th07_right_index, out th0_left_index, out th0_right_index, out th0_left, out th0_right);

        var start = 0;
        for (var i = 0; i < len; i++)
            if (pattern[i].Angle >= Consts.pi05neg)
            {
                start = i;
                break;
            }

        for (var i = start; i < len; i++)
            if (pattern[i].Angle >= Consts.pi05)
                len = i;

        pattern.FindBeamSLL(start, len, th0_left_index, th0_right_index,
            out LeftSLL, out LeftSLLIndex, out RightSLL, out RightSLLIndex,
            out SLL, out LeftMeanSLL, out RightMeanSLL, out MeanSLL);
    }

    private static void FindBeamWidth07
    (
        this PatternValue[] pattern,
        int len,
        int max_index,
        double max,
        out int th07_left_index,
        out int th07_right_index,
        out double th07_left,
        out double th07_right
    )
    {
        #region Определение левой границы луча

        th07_left_index = max_index - 1;
        while (th07_left_index >= 0 && pattern[th07_left_index].Value.Abs / max > Consts.sqrt_2_inv)
            th07_left_index--;

        th07_left = th07_left_index < 0
            ? double.NaN
            : UnMap(max / Consts.sqrt_2, pattern[th07_left_index].Angle, pattern[th07_left_index + 1].Angle, pattern[th07_left_index].Value.Abs, pattern[th07_left_index + 1].Value.Abs);

        #endregion

        #region Определение правой границы луча

        th07_right_index = max_index + 1;
        while (th07_right_index < len && pattern[th07_right_index].Value.Abs / max > Consts.sqrt_2_inv)
            th07_right_index++;

        th07_right = th07_right_index >= len
            ? double.NaN
            : UnMap(max / Consts.sqrt_2, pattern[th07_right_index].Angle, pattern[th07_right_index - 1].Angle, pattern[th07_right_index].Value.Abs, pattern[th07_right_index - 1].Value.Abs);

        #endregion
    }

    private static double UnMap(double y, double x1, double x2, double y1, double y2) => (y - y1) * (x2 - x1) / (y2 - y1) + x1;

    private static void FindBeamWidth0
    (
        this PatternValue[] pattern,
        int len,
        double max,
        int th07_left_index,
        int th07_right_index,
        out int th0_left_index,
        out int th0_right_index,
        out double th0_left,
        out double th0_right
    )
    {
        #region Поиск левой границы луча по "нулю"

        th0_left_index = th07_left_index - 1;
        if (th0_left_index <= 0) th0_left = double.NaN;
        else
        {
            var last_v = pattern[th07_left_index].Value.Abs;
            while (th0_left_index >= 0)
            {
                var v = pattern[th0_left_index].Value.Abs;
                if (v > last_v)
                {
                    th0_left_index++;
                    break;
                }
                th0_left_index--;
                last_v = v;
            }

            th0_left = th0_left_index <= 0
                ? double.NaN
                : (pattern[th0_left_index].Angle + pattern[th0_left_index + 1].Angle) / 2;
        }

        #endregion

        #region Поиск правой границы луча по "нулю"

        th0_right_index = th07_right_index + 1;
        if (th0_right_index >= len) th0_right = double.NaN;
        else
        {
            var last_v = pattern[th07_right_index].Value.Abs;
            while (th0_right_index < len)
            {
                var v = pattern[th0_right_index].Value.Abs;
                if (v > last_v)
                {
                    th0_right_index--;
                    break;
                }
                th0_right_index++;
                last_v = v;
            }

            th0_right = th0_right_index >= len
                ? double.NaN
                : (pattern[th0_right_index].Angle + pattern[th0_right_index - 1].Angle) / 2;
        }

        #endregion
    }

    private static void FindBeamSLL
    (
        this PatternValue[] pattern,
        int start,
        int len,
        int th0_left_index,
        int th0_right_index,
        out double LeftSLL,
        out int LeftSLLIndex,
        out double RightSLL,
        out int RightSLLIndex,
        out double SLL,
        out double LeftMeanSLL,
        out double RightMeanSLL,
        out double MeanSLL
    )
    {
        if (th0_left_index > start + 1)
        {
            var last_v = pattern[start].Value.Abs;
            var last_a = pattern[start].Angle;
            LeftSLLIndex = th0_left_index;
            LeftSLL      = last_v;
            LeftMeanSLL  = 0d;
            for (var i = start + 1; i < th0_left_index; i++)
            {
                var v = pattern[i].Value.Abs;
                var a = pattern[i].Angle;
                if (v > LeftSLL)
                {
                    LeftSLLIndex = i;
                    LeftSLL      = v;
                }
                LeftMeanSLL += (v + last_v) / 2 * (a - last_a);
                last_v      =  v;
                last_a      =  a;
            }
        }
        else
        {
            LeftSLL      = double.NaN;
            LeftMeanSLL  = double.NaN;
            LeftSLLIndex = start;
        }

        if (th0_right_index < len - 1)
        {
            var last_v = pattern[th0_right_index].Value.Abs;
            var last_a = pattern[th0_right_index].Angle;
            RightSLLIndex = len;
            RightSLL      = last_v;
            RightMeanSLL  = 0d;
            for (var i = th0_right_index + 1; i < len; i++)
            {
                var v = pattern[i].Value.Abs;
                var a = pattern[i].Angle;
                if (v > RightSLL)
                {
                    RightSLLIndex = i;
                    RightSLL      = v;
                }
                RightMeanSLL += (v + last_v) / 2 * (a - last_a);
                last_v       =  v;
                last_a       =  a;
            }
        }
        else
        {
            RightSLL      = double.NaN;
            RightMeanSLL  = double.NaN;
            RightSLLIndex = len;
        }

        SLL = th0_left_index > 1
            ? (th0_right_index < len - 1 ? Math.Max(LeftSLL, RightSLL) : LeftSLL)
            : (th0_right_index < len - 1 ? RightSLL : double.NaN);

        if (th0_left_index > start + 1)
        {
            var left_ubl_angle = pattern[th0_left_index].Angle - pattern[start].Angle;

            if (th0_right_index < len - 1)
            {
                var right_ubl_angle = pattern[len - 1].Angle - pattern[th0_right_index].Angle;

                MeanSLL      =  (LeftMeanSLL + RightMeanSLL) / (left_ubl_angle + right_ubl_angle);
                LeftMeanSLL  /= left_ubl_angle;
                RightMeanSLL /= right_ubl_angle;
            }
            else
            {
                LeftMeanSLL /= left_ubl_angle;
                MeanSLL     =  LeftMeanSLL;
            }
        }
        else if (th0_right_index < len - 1)
        {
            var right_ubl_angle = pattern[len - 1].Angle - pattern[th0_right_index].Angle;
            RightMeanSLL /= right_ubl_angle;
            MeanSLL      =  RightMeanSLL;
        }
        else
            MeanSLL = double.NaN;
    }
}

public struct AnalysePatternResult
{
    public readonly double Max;
    public readonly int MaxIndex;
    public readonly double MaxAngle;
    public readonly double D0;
    public readonly int Th07LeftIndex;
    public readonly int Th07RightIndex;
    public readonly double Th07Left;
    public readonly double Th07Right;
    public readonly int Th0LeftIndex;
    public readonly int Th0RightIndex;
    public readonly double Th0Left;
    public readonly double Th0Right;
    public readonly double LeftSsl;
    public readonly int LeftSLLIndex;
    public readonly double RightSLL;
    public readonly int RightSLLIndex;
    public readonly double SSL;
    public readonly double LeftMeanSLL;
    public readonly double RightMeanSLL;
    public readonly double MeanSLL;

    public AnalysePatternResult
    (
        double Max, int MaxIndex, double MaxAngle, double D0,
        int Th07LeftIndex, int Th07RightIndex, double Th07Left, double Th07Right,
        int Th0LeftIndex, int Th0RightIndex, double Th0Left, double Th0Right,
        double LeftSsl,
        int LeftSLLIndex,
        double RightSLL,
        int RightSLLIndex,
        double Ssl,
        double LeftMeanSLL,
        double RightMeanSLL,
        double MeanSLL)
    {
        this.Max            = Max;
        this.MaxIndex       = MaxIndex;
        this.MaxAngle       = MaxAngle;
        this.D0             = D0;
        this.Th07LeftIndex  = Th07LeftIndex;
        this.Th07RightIndex = Th07RightIndex;
        this.Th07Left       = Th07Left;
        this.Th07Right      = Th07Right;
        this.Th0LeftIndex   = Th0LeftIndex;
        this.Th0RightIndex  = Th0RightIndex;
        this.Th0Left        = Th0Left;
        this.Th0Right       = Th0Right;
        this.LeftSsl        = LeftSsl;
        this.LeftSLLIndex   = LeftSLLIndex;
        this.RightSLL       = RightSLL;
        this.RightSLLIndex  = RightSLLIndex;
        this.SSL            = Ssl;
        this.LeftMeanSLL    = LeftMeanSLL;
        this.RightMeanSLL   = RightMeanSLL;
        this.MeanSLL        = MeanSLL;
    }
}