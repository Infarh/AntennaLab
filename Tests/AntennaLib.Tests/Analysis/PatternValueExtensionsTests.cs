#nullable enable

using MathCore;

namespace Antennas.Tests;

/// <summary>Модульные тесты анализа массива отсчётов ДН</summary>
[TestClass]
public class PatternValueExtensionsTests
{
    private const double __Frequency = 1e9;

    /// <summary>Угол в радианах должен совпадать с исходным углом</summary>
    [TestMethod]
    public void AngleRad_ReturnsAngleInRadians()
    {
        var value = new PatternValue(Math.PI / 2, Complex.Real);

        Assert.AreEqual(Math.PI / 2, value.AngleRad, 1e-12);
    }

    /// <summary>Уровень по мощности должен совпадать с уровнем поля</summary>
    [TestMethod]
    public void ValueIndBP_MatchesFieldLevel()
    {
        var value = new PatternValue(0, new Complex(0.5, 0));

        Assert.AreEqual(value.ValueIndB, value.ValueIndBP, 1e-12);
        Assert.AreEqual(-6.020599913279624, value.ValueIndBP, 1e-12);
    }

    /// <summary>Анализ типовой ДН решётки должен возвращать конечные метрики</summary>
    [TestMethod]
    public void Analyse_LinearArray_ReturnsFiniteMetrics()
    {
        var array = new LinearAntennaArray(16, 0.15);
        var pattern = array.GetPatternPhi(__Frequency, 0, -180 * Consts.ToRad, 180 * Consts.ToRad, 1 * Consts.ToRad);

        var result = pattern.Analyse();

        Assert.IsTrue(double.IsFinite(result.Max));
        Assert.IsTrue(double.IsFinite(result.D0));
        Assert.IsGreaterThan(0, result.D0);
        Assert.IsTrue(result.MaxIndex >= 0 && result.MaxIndex < pattern.Length);
        Assert.IsTrue(double.IsFinite(result.SSL));
        Assert.IsTrue(double.IsFinite(result.MeanSLL));
    }

    /// <summary>Анализ одного отсчёта не должен бросать исключений</summary>
    [TestMethod]
    public void Analyse_SinglePoint_DoesNotThrow()
    {
        var result = new[] { new PatternValue(0, Complex.Real) }.Analyse();

        Assert.AreEqual(1, result.Max);
        Assert.AreEqual(0, result.MaxIndex);
    }

    /// <summary>Анализ ДН с максимумом на краю диапазона не должен бросать исключений</summary>
    [TestMethod]
    public void Analyse_MaximumAtEdge_DoesNotThrow()
    {
        var pattern = Enumerable.Range(0, 181)
           .Select(i => new PatternValue(-90 * Consts.ToRad + i * Consts.ToRad, new Complex(i / 180d, 0)))
           .ToArray();

        var result = pattern.Analyse();

        Assert.AreEqual(180, result.MaxIndex);
        Assert.IsTrue(double.IsFinite(result.Max));
    }

    /// <summary>Пустой массив должен приводить к явной ошибке</summary>
    [TestMethod]
    public void Analyse_EmptyArray_ThrowsInvalidOperationException() =>
        Assert.ThrowsExactly<InvalidOperationException>(() => Array.Empty<PatternValue>().Analyse());
}
