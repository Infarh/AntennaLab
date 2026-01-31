#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты для класса LambdaAntenna</summary>
[TestClass]
public class LambdaAntennaTests
{
    /// <summary>Проверка инициализации с функцией</summary>
    [TestMethod]
    public void Constructor_WithFunction_ShouldAcceptCustomFunction()
    {
        // Arrange
        Func<double, double, double, double> beam = (theta, phi, f) => Math.Cos(theta);
        
        // Act
        var antenna = new Antennas.LambdaAntenna(beam);
        
        // Assert
        Assert.IsNotNull(antenna, "Антенна должна быть создана");
    }

    /// <summary>Проверка, что функция применяется правильно</summary>
    [TestMethod]
    public void Pattern_ShouldCallCustomFunction()
    {
        // Arrange
        Func<double, double, double, double> beam = (theta, phi, f) => Math.Cos(theta);
        var antenna = new Antennas.LambdaAntenna(beam);
        var direction = new SpaceAngle(Math.PI / 4, 0);
        const double frequency = 1e9;
        
        // Act
        var result = antenna.Pattern(direction, frequency);
        var expected = Math.Cos(Math.PI / 4);
        
        // Assert
        Assert.AreEqual(expected, result.Re, 1e-10);
    }

    /// <summary>Проверка функции, зависящей от азимута</summary>
    [TestMethod]
    public void Pattern_WithPhiDependentFunction()
    {
        // Arrange
        Func<double, double, double, double> beam = (theta, phi, f) => Math.Cos(phi);
        var antenna = new Antennas.LambdaAntenna(beam);
        var direction1 = new SpaceAngle(Math.PI / 4, 0);
        var direction2 = new SpaceAngle(Math.PI / 4, Math.PI);
        const double frequency = 1e9;
        
        // Act
        var result1 = antenna.Pattern(direction1, frequency);
        var result2 = antenna.Pattern(direction2, frequency);
        
        // Assert
        Assert.AreEqual(1.0, result1.Re, 1e-10, "cos(0) должен быть 1");
        Assert.AreEqual(-1.0, result2.Re, 1e-10, "cos(π) должен быть -1");
    }

    /// <summary>Проверка функции, зависящей от частоты</summary>
    [TestMethod]
    public void Pattern_WithFrequencyDependentFunction()
    {
        // Arrange
        Func<double, double, double, double> beam = (theta, phi, f) => f / 1e9; // нормированная частота
        var antenna = new Antennas.LambdaAntenna(beam);
        var direction = new SpaceAngle(0, 0);
        const double frequency = 2e9;
        
        // Act
        var result = antenna.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(2.0, result.Re, 1e-10, "Результат должен быть 2 при f=2e9");
    }

    /// <summary>Проверка сложной функции диаграммы</summary>
    [TestMethod]
    public void Pattern_WithComplexFunction()
    {
        // Arrange
        Func<double, double, double, double> beam = (theta, phi, f) => 
            Math.Sin(theta) * Math.Cos(phi) * Math.Sin(2 * Math.PI * f / 1e9);
        var antenna = new Antennas.LambdaAntenna(beam);
        var direction = new SpaceAngle(Math.PI / 6, Math.PI / 3);
        const double frequency = 1.5e9;
        
        // Act
        var result = antenna.Pattern(direction, frequency);
        var expected = Math.Sin(Math.PI / 6) * Math.Cos(Math.PI / 3) * Math.Sin(2 * Math.PI * 1.5);
        
        // Assert
        Assert.AreEqual(expected, result.Re, 1e-10);
    }

    /// <summary>Проверка константной функции</summary>
    [TestMethod]
    public void Pattern_WithConstantFunction()
    {
        // Arrange
        Func<double, double, double, double> beam = (theta, phi, f) => 1.0;
        var antenna = new Antennas.LambdaAntenna(beam);
        
        // Act & Assert
        for (var theta = 0.0; theta <= Math.PI; theta += Math.PI / 4)
        {
            for (var phi = 0.0; phi < 2 * Math.PI; phi += Math.PI / 2)
            {
                var direction = new SpaceAngle(theta, phi);
                var result = antenna.Pattern(direction, 1e9);
                Assert.AreEqual(1.0, result.Re, 1e-10);
            }
        }
    }

    /// <summary>Проверка функции, возвращающей нулевое значение</summary>
    [TestMethod]
    public void Pattern_WithZeroFunction()
    {
        // Arrange
        Func<double, double, double, double> beam = (theta, phi, f) => 0.0;
        var antenna = new Antennas.LambdaAntenna(beam);
        var direction = new SpaceAngle(Math.PI / 4, Math.PI / 3);
        
        // Act
        var result = antenna.Pattern(direction, 1e9);
        
        // Assert
        Assert.AreEqual(0.0, result.Abs, 1e-10);
    }
}
