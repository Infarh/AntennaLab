#nullable enable

using MathCore;

namespace Antennas.Tests;

/// <summary>Модульные тесты адаптивного анализа диаграммы направленности</summary>
[TestClass]
public class BeamPatternExtensionsTests
{
    private const double __Frequency = 1e9;

    /// <summary>Анализ типовой ДН линейной решётки не должен бросать исключений</summary>
    [TestMethod]
    public void Analyze_LinearArray_ReturnsValidResult()
    {
        var array = new LinearAntennaArray(16, 0.15);
        var pattern = array.GetPatternOfThetaOnFreq(__Frequency);

        var result = pattern.Analyze();

        Assert.IsTrue(double.IsFinite(result.AngleOfMaximum));
        Assert.IsTrue(double.IsFinite(result.BeamWidth));
        Assert.IsTrue(double.IsFinite(result.Directivity));
        Assert.IsGreaterThanOrEqualTo(0, result.MaximumIndex);
    }

    /// <summary>Анализ узкой ДН не должен бросать исключений</summary>
    [TestMethod]
    public void Analyze_NarrowPattern_ReturnsValidResult()
    {
        static Complex Pattern(double theta) => Math.Cos(8 * theta);

        var result = ((Func<double, Complex>)Pattern).Analyze();

        Assert.IsTrue(double.IsFinite(result.AngleOfMaximum));
        Assert.IsGreaterThanOrEqualTo(0, result.MaximumIndex);
    }

    /// <summary>Анализ постоянной ДН не должен бросать исключений</summary>
    [TestMethod]
    public void Analyze_ConstantPattern_DoesNotThrow()
    {
        static Complex Pattern(double _) => Complex.Real;

        var result = ((Func<double, Complex>)Pattern).Analyze();

        Assert.IsTrue(double.IsFinite(result.AngleOfMaximum));
        Assert.IsGreaterThanOrEqualTo(0, result.MaximumIndex);
    }

    /// <summary>Анализ ДН с максимумом на краю диапазона не должен бросать исключений</summary>
    [TestMethod]
    public void Analyze_MaximumAtRangeEdge_DoesNotThrow()
    {
        static Complex Pattern(double theta) => theta + Math.PI;

        var result = ((Func<double, Complex>)Pattern).Analyze();

        Assert.IsTrue(double.IsFinite(result.AngleOfMaximum));
        Assert.IsGreaterThanOrEqualTo(0, result.MaximumIndex);
    }

    /// <summary>Неверная точность должна приводить к явной ошибке</summary>
    [TestMethod]
    public void Analyze_InvalidAccuracy_ThrowsArgumentOutOfRangeException() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((Func<double, Complex>)(_ => Complex.Real)).Analyze(Accuracy: 0));
}
