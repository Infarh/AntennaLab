#nullable enable

using System.Globalization;

using ArrayFactor.Service;
using ArrayFactor.Service.AmplitudeDistributions;

namespace ArrayFactor.Tests;

/// <summary>Тесты параметров модели и амплитудных распределений</summary>
[TestClass]
public class ModelParameterTests
{
    /// <summary>Сектор сканирования должен учитывать соотношение длины волны и шага</summary>
    [TestMethod]
    [DataRow(15.0, 30.0, 90.0)]
    [DataRow(10.0, 30.0, 90.0)]
    [DataRow(20.0, 30.0, 30.0)]
    [DataRow(40.0, 30.0, 0.0)]
    public void ScanSector_ReturnsExpectedAngle(double step, double wave_length, double expected)
    {
        var model = new MainWindowModel { λ0 = wave_length, dx = step };

        Assert.AreEqual(expected, model.θmax_x, 1e-9);
    }

    /// <summary>Треугольное распределение должно учитывать пьедестал</summary>
    [TestMethod]
    public void Triangle_RespectsPedestal()
    {
        var distribution = new Triangle { Dx = 0.2, Dy = 0.2 };

        Assert.AreEqual(0.2, distribution.Value(0.5, 0, 0), 1e-12);
        Assert.AreEqual(1.0, distribution.Value(0, 0, 0), 1e-12);
    }

    /// <summary>Параболическое распределение должно учитывать пьедестал</summary>
    [TestMethod]
    public void Parabolic_RespectsPedestal()
    {
        var distribution = new Parabolic { Dx = 0.2, Dy = 0.2 };

        Assert.AreEqual(0.2, distribution.Value(0.5, 0, 0), 1e-12);
        Assert.AreEqual(1.0, distribution.Value(0, 0, 0), 1e-12);
    }

    /// <summary>Чебышевское распределение должно быть максимальным в центре и спадать к краям апертуры</summary>
    [TestMethod]
    public void Chebyshev_HasMaximumAtCenter()
    {
        var distribution = new Chebyshev { SLL = 30 };

        Assert.AreEqual(1.0, distribution.Value(0, 0, 0), 1e-12);
        Assert.IsTrue(distribution.Value(0.5, 0, 0) is > 0 and < 0.2);
        Assert.AreEqual(distribution.Value(0.5, 0, 0), distribution.Value(0, 0.5, 0), 1e-12);
    }

    /// <summary>Рост подавления боковых лепестков должен уменьшать уровень поля на краю апертуры</summary>
    [TestMethod]
    public void Chebyshev_HigherSLL_ReducesEdgeLevel()
    {
        var low = new Chebyshev { SLL = 20 };
        var high = new Chebyshev { SLL = 40 };

        Assert.IsTrue(high.Value(0.5, 0, 0) < low.Value(0.5, 0, 0));
    }

    /// <summary>К моменту уведомления об изменении уровня лепестков распределение должно быть уже пересчитано</summary>
    [TestMethod]
    public void Chebyshev_PropertyChanged_ObservesUpdatedTaper()
    {
        var distribution = new Chebyshev { SLL = 20 };
        var observed = double.NaN;

        distribution.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Chebyshev.SLL)) observed = distribution.Value(0.5, 0, 0);
        };

        distribution.SLL = 40;

        Assert.AreEqual(distribution.Value(0.5, 0, 0), observed, 1e-12);
    }

    /// <summary>Распределение Тейлора должно быть максимальным в центре и спадать к краям апертуры</summary>
    [TestMethod]
    public void Taylor_HasMaximumAtCenter()
    {
        var distribution = new Taylor { SLL = 30 };

        Assert.AreEqual(1.0, distribution.Value(0, 0, 0), 1e-12);
        Assert.IsTrue(distribution.Value(0.5, 0, 0) is > 0 and < 0.2);
    }

    /// <summary>К моменту уведомления об изменении уровня лепестков распределение Тейлора должно быть пересчитано</summary>
    [TestMethod]
    public void Taylor_PropertyChanged_ObservesUpdatedTaper()
    {
        var distribution = new Taylor { SLL = 20 };
        var observed = double.NaN;

        distribution.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Taylor.SLL)) observed = distribution.Value(0.5, 0, 0);
        };

        distribution.SLL = 40;

        Assert.AreEqual(distribution.Value(0.5, 0, 0), observed, 1e-12);
    }

    /// <summary>Гауссово распределение должно обеспечивать заданный уровень поля на краю апертуры</summary>
    [TestMethod]
    public void Gaussian_EdgeLevelMatchesParameter()
    {
        var distribution = new Gaussian { Dx = 0.2, Dy = 0.3 };

        Assert.AreEqual(1.0, distribution.Value(0, 0, 0), 1e-12);
        Assert.AreEqual(0.2, distribution.Value(0.5, 0, 0), 1e-12);
        Assert.AreEqual(0.3, distribution.Value(0, 0.5, 0), 1e-12);
    }

    /// <summary>Ошибочное выражение пользовательского распределения не должно бросать исключений</summary>
    [TestMethod]
    public void UserDistribution_InvalidExpression_ReturnsNaN()
    {
        var distribution = new UserDistribution { DistributionExpression = "@" };

        Assert.IsTrue(double.IsNaN(distribution.Value(0, 0, 0)));
    }

    /// <summary>Корректное выражение пользовательского распределения должно вычисляться</summary>
    [TestMethod]
    public void UserDistribution_ValidExpression_ReturnsValue()
    {
        var distribution = new UserDistribution { DistributionExpression = "x + y" };

        Assert.AreEqual(0.3, distribution.Value(0.1, 0.2, 0), 1e-12);
    }

    /// <summary>Конвертер должен использовать инвариантную культуру в обе стороны</summary>
    [TestMethod]
    public void DoubleToStringConverter_UsesInvariantCulture()
    {
        var converter = new DoubleToStringConverter(2);
        var culture = new CultureInfo("ru-RU");

        Assert.AreEqual("1.23", converter.Convert(1.234, typeof(string), null, culture));
        Assert.AreEqual(1.23, converter.ConvertBack("1.23", typeof(double), null, culture));
    }
}
