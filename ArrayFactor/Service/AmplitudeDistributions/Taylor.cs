using static MathCore.SpecialFunctions;

using static System.Math;

namespace ArrayFactor.Service.AmplitudeDistributions;

/// <summary>Однопараметрическое распределение Тейлора</summary>
/// <remarks>Обеспечивает монотонно спадающие боковые лепестки заданного уровня</remarks>
public class Taylor : Distribution
{
    /// <summary>Уровень первого бокового лепестка равномерного распределения, дБ</summary>
    private const double __UniformSLL = 13.26;

    /// <summary>Уровень боковых лепестков в дБ</summary>
    private double _SLL = 30;

    /// <summary>Параметр распределения, равный произведению π на параметр Тейлора</summary>
    private double _B;

    /// <summary>Нормировочный множитель распределения</summary>
    private double _Norm;

    public Taylor()
    {
        _B    = GetParameter(_SLL);
        _Norm = 1 / Bessel.I0(_B);
    }

    /// <summary>Уровень боковых лепестков в дБ (положительное значение соответствует подавлению)</summary>
    public double SLL
    {
        get => _SLL;
        set
        {
            if (value is < 13.3 or > 80) return;
            // параметры обновляются до уведомления, чтобы подписчики видели согласованное состояние
            _B    = GetParameter(value);
            _Norm = 1 / Bessel.I0(_B);
            Set(ref _SLL, value);
        }
    }

    /// <summary>Вычисление параметра распределения по заданному уровню боковых лепестков</summary>
    /// <param name="SLL">Уровень боковых лепестков в дБ</param>
    /// <returns>Параметр распределения, равный π·A</returns>
    /// <remarks>Параметр находится из решения уравнения sinh(b)/b = 10^((SLL - 13.26)/20)</remarks>
    private static double GetParameter(double SLL)
    {
        var target = Pow(10, (SLL - __UniformSLL) / 20);
        double lo = 1e-6, hi = 1;
        while (Sinh(hi) / hi < target) hi *= 2;

        for (var i = 0; i < 60; i++)
        {
            var mid = (lo + hi) / 2;
            if (Sinh(mid) / mid < target) lo = mid;
            else hi = mid;
        }

        return (lo + hi) / 2;
    }

    /// <summary>Значение одномерного распределения в нормированной координате апертуры</summary>
    /// <param name="x">Нормированная координата в пределах от -0.5 до 0.5</param>
    private double A(double x) => Bessel.I0(_B * Sqrt(Max(0, 1 - 4 * x * x))) * _Norm;

    /// <inheritdoc />
    public override double Value(double x, double y, double z) => A(x) * A(y);

    /// <inheritdoc />
    public override string ToString() => $"Тейлоровское (УБЛ {_SLL:0.#} дБ)";
}
