#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты для класса Vibrator</summary>
[TestClass]
public class VibratorTests
{
    /// <summary>Проверка инициализации вибратора с длиной</summary>
    [TestMethod]
    public void Constructor_WithLength_ShouldSetLength()
    {
        // Arrange
        const double expected_length = 0.5;
        
        // Act
        var vibrator = new Antennas.Vibrator(expected_length);
        
        // Assert
        Assert.AreEqual(expected_length, vibrator.Length, 1e-10);
    }

    /// <summary>Проверка конструктора по умолчанию</summary>
    [TestMethod]
    public void Constructor_Default_ShouldSetLengthTo1()
    {
        // Arrange & Act
        var vibrator = new Antennas.Vibrator();
        
        // Assert
        Assert.AreEqual(1.0, vibrator.Length, 1e-10);
    }

    /// <summary>Проверка возможности изменения длины вибратора</summary>
    [TestMethod]
    public void Length_CanBeChanged()
    {
        // Arrange
        var vibrator = new Antennas.Vibrator(0.5);
        const double new_length = 0.7;
        
        // Act
        vibrator.Length = new_length;
        
        // Assert
        Assert.AreEqual(new_length, vibrator.Length, 1e-10);
    }

    /// <summary>Проверка значения диаграммы направленности на зените</summary>
    [TestMethod]
    public void Pattern_AtZenith_ShouldReturn0()
    {
        // Arrange
        var vibrator = new Antennas.Vibrator(0.5);
        var direction = new SpaceAngle(0, 0);
        const double frequency = 1e9;
        
        // Act
        var result = vibrator.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(0.0, result.Abs, 1e-10, "Вибратор на зените должен иметь 0");
    }

    /// <summary>Проверка, что диаграмма вибратора зависит от длины</summary>
    [TestMethod]
    public void Pattern_DependsOnLength()
    {
        // Arrange
        var vibrator1 = new Antennas.Vibrator(0.5);
        var vibrator2 = new Antennas.Vibrator(1.0);
        var direction = new SpaceAngle(Math.PI / 4, 0);
        const double frequency = 1e9;
        
        // Act
        var result1 = vibrator1.Pattern(direction, frequency);
        var result2 = vibrator2.Pattern(direction, frequency);
        
        // Assert
        Assert.AreNotEqual(result1, result2, "Вибраторы разной длины должны иметь разные диаграммы");
    }

    /// <summary>Проверка получения эффективной длины вибратора</summary>
    [TestMethod]
    public void GetActiveLength_ShouldReturnValue()
    {
        // Arrange
        var vibrator = new Antennas.Vibrator(0.5);
        const double frequency = 1e9;
        
        // Act
        var active_length = vibrator.GetActiveLength(frequency);
        
        // Assert
        Assert.IsFalse(double.IsNaN(active_length), "Эффективная длина не должна быть NaN");
    }

    /// <summary>Проверка распределения тока вибратора</summary>
    [TestMethod]
    public void CurrentDistribution_ShouldReturnValues()
    {
        // Arrange
        var vibrator = new Antennas.Vibrator(0.5);
        const double frequency = 1e9;
        const double center = 0;
        const double offset = 0.1;
        
        // Act
        var current_center = vibrator.CurrentDistribution(frequency, center);
        var current_offset = vibrator.CurrentDistribution(frequency, offset);
        
        // Assert
        Assert.IsFalse(double.IsNaN(current_center), "Ток в центре не должен быть NaN");
        Assert.IsFalse(double.IsNaN(current_offset), "Ток со смещением не должен быть NaN");
    }

    /// <summary>Проверка static метода GetWaveImpedance</summary>
    [TestMethod]
    public void GetWaveImpedance_ShouldReturnPositiveValue()
    {
        // Arrange
        const double d = 0.001; // 1 мм
        const double D = 0.1;   // 10 см
        
        // Act
        var impedance = Antennas.Vibrator.GetWaveImpedance(d, D);
        
        // Assert
        Assert.IsGreaterThan(0, impedance, "Волновое сопротивление должно быть положительным");
    }

    /// <summary>Проверка метода GetRadiatingImpedance</summary>
    [TestMethod]
    public void GetRadiatingImpedance_ShouldReturnPositiveValue()
    {
        // Arrange
        const double length = 0.5;
        const double f0 = 1e9;
        
        // Act
        var impedance = Antennas.Vibrator.GetRadiatingImpedance(length, f0);
        
        // Assert
        Assert.IsGreaterThan(0, impedance, "Сопротивление излучения должно быть положительным");
    }

    /// <summary>Проверка метода GetInputImpedance</summary>
    [TestMethod]
    public void GetInputImpedance_ShouldReturnNonZeroValue()
    {
        // Arrange
        const double length = 0.5;
        const double rho = 50; // волновое сопротивление в Оmahх
        const double f0 = 1e9;
        
        // Act
        var impedance = Antennas.Vibrator.GetInputImpedance(length, rho, f0);
        
        // Assert
        Assert.AreNotEqual(0, impedance, "Входной импеданс не должен быть нулевым");
    }

    /// <summary>Проверка ToString</summary>
    [TestMethod]
    public void ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var vibrator = new Antennas.Vibrator(0.5);
        
        // Act
        var name = vibrator.ToString();
        
        // Assert
        Assert.IsNotNull(name, "ToString должен вернуть значение");
        Assert.IsTrue(name.Contains("Вибратор") || name.Contains("Vibrator"), 
            "Результат должен содержать название антенны");
    }
}
