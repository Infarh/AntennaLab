using System.Linq.Expressions;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;
using static System.Math;
using static MathCore.Extensions.Expressions.MathExpression;

namespace Antennas;

/// <summary>Симметричный вибратор расположенный вдоль оси OZ</summary>
public class Vibrator : Antenna
{
    private const double __k = Consts.pi2 / Consts.SpeedOfLight;

    private static double GetK_f(double f) => __k * f;

    private static double GetK_l(double l) => Consts.pi2 / l;

    public static double GetRadiatingImpedance(double Length, double f0)
    {
        var kl     = GetK_f(f0) * Length;
        var cos_kl = Cos(kl);

        double Theta(double theta) => Cos(kl * Cos(theta)) - cos_kl;
        Func<double, double> F = Theta;
        static double Core(double f, double theta) => theta.Equals(0) ? 1 : f * f / Sin(theta);
        return 60 * F.GetIntegralValue(Core, 0, Consts.pi, Consts.pi / 10000);
    }

    public static Complex GetInputImpedance(double Length, double rho, double f0)
    {
        var k       = GetK_f(f0);
        var l2_4    = 4 * Length * Length;
        var cos2_kl = Cos(2 * k * Length);
        var m       = l2_4 * cos2_kl - cos2_kl + l2_4 + 1;
        var re      = 4 * rho * Length / m;
        var im      = rho * Sin(2 * k * Length) * (l2_4 - 1) / m;
        return new Complex(re, im);
    }

    public static double CurrentDistribution(double f0, double Length, double z) => Sin(GetK_f(f0) * (Length - Abs(z)));

    public static double GetWaveImpedance(double d, double D) => 120 * (Log(D / d) - 1);

    private double _Length;

    public double Length
    {
        get => _Length;
        set => Set(ref _Length, value);
    }

    public Vibrator() : this(1) { }

    public Vibrator(double Length) => _Length = Length;

    public override Complex Pattern(SpaceAngle Direction, double f)
    {
        var th     = Direction.ThetaRad;
        var kl     = __k * f * _Length;
        var cos_kl = Cos(kl);
        return th.Equals(0) ? 0 : (Cos(kl * Cos(th)) - cos_kl) / /*(1 - cos_kl) /*/ Sin(th);
    }

    public override Expression GetPatternExpressionBody(Expression a, Expression f)
    {
        var th = a.GetProperty(nameof(SpaceAngle.ThetaRad));
        var kl = __k.ToExpression()
           .Multiply(f)
           .Multiply(this.ToExpression().GetField(nameof(_Length)));
        var cos_kl = Cos(kl);
        return Cos(kl.Multiply(Cos(th))).Subtract(cos_kl)
           .Divide(1d.ToExpression().Subtract(cos_kl).Multiply(Sin(th)));
    }
        
    public double GetActiveLength(double f)
    {
        const double k   = Consts.pi / Consts.SpeedOfLight;
        var          k05 = k * f;
        return Tan(k05 * _Length) / k05;
    }
        
    public double CurrentDistribution(double f0, double z) => CurrentDistribution(f0, _Length, z);

    public Func<double, double> CurrentDistribution(double f0) => z => CurrentDistribution(f0, z);

    public override string ToString() => $"Вибратор L={Length}";
}