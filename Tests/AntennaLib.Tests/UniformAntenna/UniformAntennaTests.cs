#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты для класса UniformAntenna</summary>
[TestClass]
public class UniformAntennaTests
{
    /// <summary>Проверка, что всенаправленная антенна возвращает 1 для любого направления</summary>
    [TestMethod]
    public void Pattern_ShouldAlwaysReturn1()
    {
        // Arrange
        var antenna = new Antennas.UniformAntenna();
        
        // Act & Assert
        for (var theta = 0.0; theta <= Math.PI; theta += Math.PI / 6)
        {
            for (var phi = 0.0; phi < 2 * Math.PI; phi += Math.PI / 3)
            {
                var direction = new SpaceAngle(theta, phi);
                var result = antenna.Pattern(direction, 1e9);
                
                Assert.AreEqual(1.0, result.Abs, 1e-10, 
                    $"Диаграмма должна быть 1 для θ={theta}, φ={phi}");
            }
        }
    }

    /// <summary>Проверка независимости от частоты</summary>
    [TestMethod]
    [DataRow(1e6)]
    [DataRow(1e9)]
    [DataRow(1e12)]
    public void Pattern_ShouldNotDependOnFrequency(double frequency)
    {
        // Arrange
        var antenna = new Antennas.UniformAntenna();
        var direction = new SpaceAngle(Math.PI / 4, Math.PI / 3);
        
        // Act
        var result = antenna.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(1.0, result.Abs, 1e-10, 
            $"Диаграмма должна быть 1 для частоты {frequency}");
    }

    /// <summary>Проверка изолированности экземпляров</summary>
    [TestMethod]
    public void MultipleInstances_ShouldWork()
    {
        // Arrange
        var antenna1 = new Antennas.UniformAntenna();
        var antenna2 = new Antennas.UniformAntenna();
        var direction = new SpaceAngle(Math.PI / 6, Math.PI / 4);
        
        // Act
        var result1 = antenna1.Pattern(direction, 1e9);
        var result2 = antenna2.Pattern(direction, 1e9);
        
        // Assert
        Assert.AreEqual(result1, result2, "Разные экземпляры должны давать одинаковые результаты");
    }

    /// <summary>Проверка ToString</summary>
    [TestMethod]
    public void ToString_ShouldReturnCorrectName()
    {
        // Arrange
        var antenna = new Antennas.UniformAntenna();
        
        // Act
        var name = antenna.ToString();
        
        // Assert
        Assert.AreEqual("Всенаправленная антенна", name);
    }

    /// <summary>Проверка, что класс sealed и не может быть наследован</summary>
    [TestMethod]
    public void UniformAntenna_ShouldBeSealed()
    {
        // Arrange & Act & Assert
        var type = typeof(Antennas.UniformAntenna);
        Assert.IsTrue(type.IsSealed, "Класс UniformAntenna должен быть запечатан (sealed)");
    }
}
