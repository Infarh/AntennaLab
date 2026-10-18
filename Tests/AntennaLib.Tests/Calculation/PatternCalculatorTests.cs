#nullable enable

using Antennas.Calculation;

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты вычислительного сервиса</summary>
[TestClass]
public class PatternCalculatorTests
{
    private const double __Frequency = 1e9;
    private const double __WaveLength = Consts.SpeedOfLight / __Frequency;

    private static PatternRequest CreateRequest(int n = 16, int heat_map_size = 36) =>
        new(
            new LinearAntennaArray(n, __WaveLength / 2),
            new UniformAntenna(),
            __Frequency,
            0,
            -180 * Consts.ToRad,
            180 * Consts.ToRad,
            1 * Consts.ToRad,
            heat_map_size,
            5,
            -80);

    /// <summary>Расчёт должен возвращать сечения, тепловую карту и анализ</summary>
    [TestMethod]
    public void Calculate_LinearArray_ReturnsCompleteResult()
    {
        var request = CreateRequest();

        var result = PatternCalculator.Calculate(request);

        Assert.IsNotEmpty(result.ArrayPattern);
        Assert.IsNotEmpty(result.ElementPattern);
        Assert.AreEqual(36, result.HeatMap.GetLength(0));
        Assert.AreEqual(36, result.HeatMap.GetLength(1));
        Assert.HasCount(361, result.ArrayPattern);
        Assert.AreEqual(0, result.Analysis.MaxAngle, 1 * Consts.ToRad);
    }

    /// <summary>Тепловая карта не должна содержать NaN внутри видимой области</summary>
    [TestMethod]
    public void CalculateHeatMap_VisibleRegion_HasNoNaN()
    {
        var request = CreateRequest(heat_map_size: 36);

        var heat_map = PatternCalculator.CalculateHeatMap(request);

        for (var i = 0; i < 36; i++)
            for (var j = 0; j < 36; j++)
            {
                var angle = new Complex(18 - i, j - 18);
                if (angle.Abs * request.HeatMapAngleStepDeg <= 90)
                    Assert.IsFalse(double.IsNaN(heat_map[i, j]), $"NaN в точке ({i},{j})");
            }
    }

    /// <summary>Отменённый расчёт должен прерываться</summary>
    [TestMethod]
    public void Calculate_Cancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsExactly<OperationCanceledException>(() => PatternCalculator.Calculate(CreateRequest(), null, cts.Token));
    }

    /// <summary>Неверный шаг должен приводить к явной ошибке</summary>
    [TestMethod]
    public void Calculate_InvalidStep_ThrowsArgumentOutOfRangeException()
    {
        var request = CreateRequest() with { ThetaStep = 0 };

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PatternCalculator.Calculate(request));
    }

    /// <summary>Расчёт для пустой решётки не должен бросать исключений</summary>
    [TestMethod]
    public void Calculate_EmptyArray_DoesNotThrow()
    {
        var request = CreateRequest() with { Antenna = new AntennaArray([]) };

        var result = PatternCalculator.Calculate(request);

        Assert.IsNotEmpty(result.ArrayPattern);
        Assert.IsTrue(double.IsNaN(result.ArrayPattern[0].Value.Re));
    }
}
