using static System.Math;

using MathCore;

namespace Antennas.Calculation;

/// <summary>Вычислительный сервис расчёта диаграмм направленности, не зависящий от UI</summary>
public static class PatternCalculator
{
    /// <summary>Рассчитать сечения ДН, тепловую карту и метрики</summary>
    /// <param name="Request">Снимок входных данных</param>
    /// <param name="Progress">Информатор прогресса тепловой карты</param>
    /// <param name="Cancel">Маркер отмены</param>
    /// <returns>Результат расчёта</returns>
    /// <exception cref="ArgumentNullException">Если <paramref name="Request"/> равен <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Если шаг или размер тепловой карты заданы неверно</exception>
    public static PatternResult Calculate(PatternRequest Request, IProgress<double>? Progress = null, CancellationToken Cancel = default)
    {
        ArgumentNullException.ThrowIfNull(Request);
        if (!double.IsFinite(Request.ThetaStep) || Request.ThetaStep <= 0)
            throw new ArgumentOutOfRangeException(nameof(Request), "Шаг по углу должен быть положительным конечным числом");
        if (Request.HeatMapSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(Request), "Размер тепловой карты должен быть положительным");

        Cancel.ThrowIfCancellationRequested();

        var array_pattern = Request.Antenna.GetPatternValuesParallel(
            Request.Frequency, Request.Phi, Request.ThetaMin, Request.ThetaMax, Request.ThetaStep, Cancel);

        Cancel.ThrowIfCancellationRequested();

        var element_pattern = Request.Element.GetPatternValuesParallel(
            Request.Frequency, Request.Phi, Request.ThetaMin, Request.ThetaMax, Request.ThetaStep, Cancel);

        Cancel.ThrowIfCancellationRequested();

        var analysis = array_pattern.Analyse();
        var heat_map = CalculateHeatMap(Request, Progress, Cancel);

        return new(array_pattern, element_pattern, heat_map, analysis);
    }

    /// <summary>Рассчитать тепловую карту пространственной ДН</summary>
    /// <param name="Request">Снимок входных данных</param>
    /// <param name="Progress">Информатор прогресса</param>
    /// <param name="Cancel">Маркер отмены</param>
    /// <returns>Нормированная тепловая карта в дБ</returns>
    public static double[,] CalculateHeatMap(PatternRequest Request, IProgress<double>? Progress = null, CancellationToken Cancel = default)
    {
        ArgumentNullException.ThrowIfNull(Request);

        var size = Request.HeatMapSize;
        var angle_step = Request.HeatMapAngleStepDeg;
        var data = new double[size, size];
        var max = double.NegativeInfinity;
        var half = size / 2d;

        Progress?.Report(0);

        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                var angle = new Complex(half - i, j - half);
                var theta_deg = angle.Abs * angle_step;
                if (theta_deg > 90)
                {
                    data[i, j] = double.NaN;
                    continue;
                }

                var value = Request.Antenna.Pattern(theta_deg * Consts.ToRad, angle.Arg, Request.Frequency)
                   .Power.In_dB_byPower();
                data[i, j] = value;
                max = Max(value, max);
            }

            Cancel.ThrowIfCancellationRequested();
            Progress?.Report((i + 1d) / size);
        }

        if (double.IsInfinity(max)) return data;

        for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                var value = data[i, j];
                data[i, j] = double.IsNaN(value) ? double.NaN : Max(value - max, Request.HeatMapMinDb);
            }

        Progress?.Report(Cancel.IsCancellationRequested ? 0 : 1);
        Cancel.ThrowIfCancellationRequested();
        return data;
    }
}
