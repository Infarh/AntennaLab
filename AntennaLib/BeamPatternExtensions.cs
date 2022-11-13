using MathCore;
using MathCore.Vectors;
using Task = System.Threading.Tasks.Task;

namespace Antennas;

public static class BeamPatternExtensions
{
    public class BeamAnalysandResult
    {
        private readonly FuncExtensions.SamplingResult<Complex>.Result[] _Samples;
        private readonly int _Left07Index;
        private readonly int _Right07Index;
        private readonly int _LeftSllIndex;
        private readonly int _RightSllIndex;

        public int MaximumIndex { get; }

        public double AngleOfMaximum => _Samples[MaximumIndex].Argument;

        public Complex ValueOfMaxMaximum => _Samples[MaximumIndex].Value;

        public (double, Complex) Maximum => _Samples[MaximumIndex];

        public double BeamWidth => _Samples[_Right07Index].Argument - _Samples[_Left07Index].Argument;

        public double BeamCenterAngle => (_Samples[_Right07Index].Argument + _Samples[_Left07Index].Argument) / 2;

        public double Simetry
        {
            get
            {
                var result = (Maximum.Item1 - BeamCenterAngle) / BeamWidth;
                return result * result;
            }
        }

        public (double, Complex) LeftSideLobe => _Samples[_LeftSllIndex];

        public (double, Complex) RightSideLobe => _Samples[_RightSllIndex];

        public double LeftAverageSideLobLevel { get; }

        public double RightAverageSideLobLevel { get; }

        public double Directivity { get; }

        public double Accuracy { get; }

        public BeamAnalysandResult(
            FuncExtensions.SamplingResult<Complex>.Result[] Samples,
            int MaximumIndex,
            int Left07Index,
            int Right07Index,
            int LeftSLLIndex, int RightSLLIndex,
            double LeftAverageSLL, double RightAverageSLL,
            double Directivity,
            double Accuracy)
        {
            _Samples                 = Samples;
            this.MaximumIndex        = MaximumIndex;
            _Left07Index             = Left07Index;
            _Right07Index            = Right07Index;
            _LeftSllIndex            = LeftSLLIndex;
            _RightSllIndex           = RightSLLIndex;
            LeftAverageSideLobLevel  = LeftAverageSLL;
            RightAverageSideLobLevel = RightAverageSLL;
            this.Directivity         = Directivity;
            this.Accuracy            = Accuracy;
        }
    }

    public static BeamAnalysandResult Analyze(
        this Func<double, Complex> f,
        double AngleMin = -Consts.pi, double AngleMax = Consts.pi,
        double Accuracy = 1e-2)
    {
        var ff = f.SamplingAdaptive_OneWay(fv => fv.Power, AngleMin, AngleMax, Accuracy).ToArray();

        var max_pos = ff.GetMaxIndex(fv => fv.Value.Power);

        var max2  = ff[max_pos].Value.Power;
        var ff_2  = Array.ConvertAll(ff, v => v.Value.Power / max2);
        var ff_db = Array.ConvertAll(ff_2, v => v.In_dB_byPower());

        var index_07_left  = 0;
        var index_07_right = 0;
        var length         = ff.Length;
        for(int i = 0, i_left = max_pos, i_right = max_pos; i < length && (index_07_left == 0 || index_07_right == 0); i++)
        {
            if(index_07_left == 0)
            {
                i_left--;
                if(i_left <= 0) index_07_left = 0;
                else
                {
                    var left = ff_db[i_left];
                    if(left < -3)
                        index_07_left = -3 - left < ff_db[i_left + 1] + 3 ? i_left : i_left + 1;
                }
            }

            if(index_07_right == 0)
            {
                i_right++;
                if(i_right >= length - 1) index_07_right = length - 1;
                else
                {
                    var right = ff_db[i_right];
                    if(right < -3)
                        index_07_right = -3 - right < ff_db[i_right - 1] + 3 ? i_right : i_right + 1;
                }
            }
        }

        var max_angle  = ff[max_pos].Argument;
        var beam_width = ff[index_07_right].Argument - ff[index_07_left].Argument;

        var sll_left_average   = 0d;
        var sll_left_max_index = 0;
        var sll_left_max       = double.NegativeInfinity;
        var index              = index_07_left;
        while(ff[index].Argument > max_angle - beam_width && index >= 0) index--;
        for(var i = index; i >= 0; i--)
        {
            var fv = ff_2[i];
            sll_left_average += fv;
            if(fv <= sll_left_max) continue;
            sll_left_max       = fv;
            sll_left_max_index = i;
        }
        sll_left_average /= index;

        var sll_right_average   = 0d;
        var sll_right_max_index = 0;
        var sll_right_max       = double.NegativeInfinity;
        index = index_07_right;
        while(ff[index].Argument > max_angle + beam_width && index < length) index++;
        for(var i = index; i < length; i++)
        {
            var fv = ff_2[i];
            sll_right_average += fv;
            if(fv <= sll_right_max) continue;
            sll_right_max       = fv;
            sll_right_max_index = i;
        }
        sll_right_average /= length - index;

        return new BeamAnalysandResult
        (
            ff, max_pos,
            index_07_left, index_07_right,
            sll_left_max_index, sll_right_max_index,
            sll_left_average, sll_right_average,
            GetDirectivity(ff, max_pos),
            Accuracy
        );
    }

    private static double GetDirectivity(FuncExtensions.SamplingResult<Complex>.Result[] ff, double max_pos)
    {
        var d    = 0d;
        var last = ff[0].Value.Power * Math.Cos(ff[0].Argument - max_pos);
        for(var i = 1; i < ff.Length; i++)
        {
            var f = ff[i].Value.Power * Math.Cos(ff[i].Argument - max_pos);
            d    += (ff[i].Argument - ff[i - 1].Argument) * (f + last) / 2;
            last =  f;
        }
        var a = ff[^1].Argument - ff[0].Argument;
        return a * a / d;
    }

    public static Func<double, Complex> ToThetaPattern(Func<SpaceAngle, Complex> f, double Phi) => Theta => f(new SpaceAngle(Theta, Phi));

    public static Func<double, Complex> ToPhiPattern(Func<SpaceAngle, Complex> f, double Theta) => Phi => f(new SpaceAngle(Theta, Phi));

    public static double GetDirectivity(this Func<SpaceAngle, Complex> F, Action<double>? Complite = null) =>
        GetDirectivity((th, phi) => F(new SpaceAngle(th, phi)), Complite);

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
        GetDirectivityBuffered((th, phi) => F(new SpaceAngle(th, phi)), Complite);

    public static double GetDirectivityBuffered(this Func<double, double, Complex> F, Action<double>? Complite = null)
    {
        var i_p    = 0;
        var N_p    = 360;
        var N_t    = 360;
        var buffer = new Dictionary<double, Dictionary<double, double>>(360);

        var f = Complite != null
            ? (Func<double, double>)(p =>
            {
                var b = buffer.GetValueOrAddNew(p, () => new Dictionary<double, double>(360));
                i_p++;
                Func<double, double> f0 = t => b.GetValueOrAddNew(t, th => F(th, p).Power * Math.Sin(th));
                var                  I1 = f0.GetIntegralValue(0, Consts.pi, Consts.pi / N_t);
                Complite.Invoke((double)i_p / (N_p + 1));
                return I1;
            })
            : p =>
            {
                var                  b  = buffer.GetValueOrAddNew(p, () => new Dictionary<double, double>(360));
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