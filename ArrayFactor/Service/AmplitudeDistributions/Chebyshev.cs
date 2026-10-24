using static System.Math;

namespace ArrayFactor.Service.AmplitudeDistributions;

/// <summary>Чебышевское (косинус-гиперболическое) амплитудное распределение</summary>
/// <remarks>При заданном уровне боковых лепестков обеспечивает минимальную ширину главного лепестка</remarks>
public class Chebyshev : Distribution
{
    /// <summary>Таблица соответствия уровня боковых лепестков (дБ) параметру распределения</summary>
    private static readonly (double SLL, double A)[] __Table =
    [
        (13.26, 0.0000), (14, 0.5132), (16, 1.0337), (18, 1.4252), (20, 1.7851),
        (22, 2.1384), (24, 2.4953), (26, 2.8588), (28, 3.2276), (30, 3.5981),
        (32, 3.9661), (34, 4.3283), (36, 4.6828), (38, 5.0284), (40, 5.3651),
        (45, 6.1700), (50, 6.9302), (55, 7.6564), (60, 8.3580), (70, 9.7165), (80, 11.0468),
    ];

    /// <summary>Уровень боковых лепестков в дБ</summary>
    private double _SLL = 30;

    /// <summary>Параметр косинус-гиперболического распределения</summary>
    private double _A;

    /// <summary>Нормировочный множитель распределения</summary>
    private double _Norm;

    public Chebyshev()
    {
        _A    = GetParameter(_SLL);
        _Norm = 1 / Cosh(_A);
    }

    /// <summary>Уровень боковых лепестков в дБ (положительное значение соответствует подавлению)</summary>
    public double SLL
    {
        get => _SLL;
        set
        {
            if (value is < 13.3 or > 80) return;
            // параметры обновляются до уведомления, чтобы подписчики видели согласованное состояние
            _A    = GetParameter(value);
            _Norm = 1 / Cosh(_A);
            Set(ref _SLL, value);
        }
    }

    /// <summary>Вычисление параметра распределения по заданному уровню боковых лепестков</summary>
    /// <param name="SLL">Уровень боковых лепестков в дБ</param>
    /// <returns>Параметр косинус-гиперболического распределения</returns>
    private static double GetParameter(double SLL)
    {
        if (SLL <= __Table[0].SLL) return __Table[0].A;

        for (var i = 1; i < __Table.Length; i++)
        {
            var (sll, a) = __Table[i];
            if (SLL > sll) continue;
            var (sll_prev, a_prev) = __Table[i - 1];
            return a_prev + (a - a_prev) * (SLL - sll_prev) / (sll - sll_prev);
        }

        // за пределами таблицы — линейная экстраполяция по двум последним точкам
        var (sll_last, a_last) = __Table[^1];
        var (sll_prev_last, a_prev_last) = __Table[^2];
        return a_last + (a_last - a_prev_last) * (SLL - sll_last) / (sll_last - sll_prev_last);
    }

    /// <summary>Значение одномерного распределения в нормированной координате апертуры</summary>
    /// <param name="x">Нормированная координата в пределах от -0.5 до 0.5</param>
    private double A(double x) => Cosh(_A * Sqrt(Max(0, 1 - 4 * x * x))) * _Norm;

    /// <inheritdoc />
    public override double Value(double x, double y, double z) => A(x) * A(y);

    /// <inheritdoc />
    public override string ToString() => $"Чебышевское (УБЛ {_SLL:0.#} дБ)";
}
