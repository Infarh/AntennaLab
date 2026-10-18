using MathCore;
using static System.Math;

// ReSharper disable once CheckNamespace
namespace Antennas;

/// <summary>Расширяющие методы для работы с антеннами</summary>
public static class AntennaExtensions
{
    /// <summary>Сформировать равномерную сетку углов, гарантированно включающую конечный угол</summary>
    /// <param name="th1">Первый угол</param>
    /// <param name="th2">Второй угол</param>
    /// <param name="dth">Шаг сетки</param>
    /// <returns>Массив углов</returns>
    /// <exception cref="ArgumentOutOfRangeException">Если шаг не положителен или не конечен</exception>
    private static double[] GetAngles(double th1, double th2, double dth)
    {
        if (!double.IsFinite(th1) || !double.IsFinite(th2))
            throw new ArgumentOutOfRangeException(nameof(th1), "Границы диапазона углов должны быть конечными");
        if (!double.IsFinite(dth) || dth <= 0)
            throw new ArgumentOutOfRangeException(nameof(dth), "Шаг по углу должен быть положительным конечным числом");

        var min = Min(th1, th2);
        var max = Max(th1, th2);
        var count = (int)Ceiling((max - min) / dth) + 1;
        var result = new double[count];
        for (var i = 0; i < count; i++)
            result[i] = Min(min + i * dth, max);
        result[^1] = max;
        return result;
    }

    /// <summary>Получить диаграмму направленности антенны на фиксированном азимутальном угле</summary>
    /// <param name="antenna">Антенна</param>
    /// <param name="f">Частота</param>
    /// <param name="phi">Азимутальный угол (по умолчанию 0)</param>
    /// <param name="th1">Начальный угол места (по умолчанию -π)</param>
    /// <param name="th2">Конечный угол места (по умолчанию π)</param>
    /// <param name="dth">Шаг по углу места (по умолчанию 1°)</param>
    /// <returns>Массив значений диаграммы направленности</returns>
    public static PatternValue[] GetPatternPhi
    (
        this Antenna antenna,
        double f,
        double phi = 0 * Consts.ToRad,
        double th1 = -180 * Consts.ToRad,
        double th2 = +180 * Consts.ToRad,
        double dth = 1 * Consts.ToRad
    )
    {
        ArgumentNullException.ThrowIfNull(antenna);

        var angles = GetAngles(th1, th2, dth);
        var result = new PatternValue[angles.Length];
        for (var i = 0; i < angles.Length; i++)
            result[i] = new(angles[i], antenna.Pattern(angles[i], phi, f));
        return result;
    }

    /// <summary>Получить диаграмму направленности антенны параллельно</summary>
    /// <param name="antenna">Антенна</param>
    /// <param name="f">Частота</param>
    /// <param name="phi">Азимутальный угол (по умолчанию 0)</param>
    /// <param name="th1">Начальный угол места (по умолчанию -π)</param>
    /// <param name="th2">Конечный угол места (по умолчанию π)</param>
    /// <param name="dth">Шаг по углу места (по умолчанию 1°)</param>
    /// <param name="Cancel">Маркер отмены</param>
    /// <returns>Массив значений диаграммы направленности</returns>
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
        ArgumentNullException.ThrowIfNull(antenna);

        var angles = GetAngles(th1, th2, dth);
        var parallel_query = angles.AsParallel().AsOrdered();
        if (Cancel.CanBeCanceled)
            parallel_query = parallel_query.WithCancellation(Cancel);
        return parallel_query.Select(th => new PatternValue(th, antenna.Pattern(th, phi, f))).ToArray();
    }

    /// <summary>Получить диаграмму направленности антенны асинхронно</summary>
    /// <param name="antenna">Антенна</param>
    /// <param name="f">Частота</param>
    /// <param name="phi">Азимутальный угол (по умолчанию 0)</param>
    /// <param name="th1">Начальный угол места (по умолчанию -π)</param>
    /// <param name="th2">Конечный угол места (по умолчанию π)</param>
    /// <param name="dth">Шаг по углу места (по умолчанию 1°)</param>
    /// <param name="Progress">Объект отчёта о прогрессе</param>
    /// <param name="Cancel">Маркер отмены</param>
    /// <returns>Массив значений диаграммы направленности</returns>
    public static Task<PatternValue[]> GetPatternPhiAsync
    (
        this Antenna antenna,
        double f,
        double phi = 0 * Consts.ToRad,
        double th1 = -180 * Consts.ToRad,
        double th2 = +180 * Consts.ToRad,
        double dth = 1 * Consts.ToRad,
        IProgress<PatternCalculationTaskProgressInfo>? Progress = null,
        CancellationToken Cancel = default
    )
    {
        ArgumentNullException.ThrowIfNull(antenna);

        return Task.Run(() =>
        {
            var angles = GetAngles(th1, th2, dth);
            var result = new PatternValue[angles.Length];
            for (var i = 0; i < angles.Length; i++)
            {
                Cancel.ThrowIfCancellationRequested();
                var pattern_value = new PatternValue(angles[i], antenna.Pattern(angles[i], phi, f));
                result[i] = pattern_value;
                Progress?.Report(new((i + 1d) / angles.Length, pattern_value));
            }
            return result;
        }, Cancel);
    }
}