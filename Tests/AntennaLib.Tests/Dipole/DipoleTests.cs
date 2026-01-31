#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты для класса Dipole</summary>
[TestClass]
public class DipoleTests
{
    /// <summary>Проверка значения диаграммы направленности диполя в направлении на север</summary>
    [TestMethod]
    public void Pattern_AtZenith_ShouldReturn1()
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        var direction = new SpaceAngle(0, 0); // Зенит
        const double frequency = 1e9;
        
        // Act
        var result = dipole.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(1.0, result.Re, 1e-10, "На зените диаграмма диполя должна быть 1");
        Assert.AreEqual(0.0, result.Im, 1e-10, "Мнимая часть должна быть 0");
    }

    /// <summary>Проверка значения диаграммы направленности диполя на горизонте</summary>
    [TestMethod]
    public void Pattern_AtHorizon_ShouldReturn0()
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        var direction = new SpaceAngle(Math.PI / 2, 0); // Горизонт
        const double frequency = 1e9;
        
        // Act
        var result = dipole.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(0.0, result.Abs, 1e-10, "На горизонте диаграмма диполя должна быть 0");
    }

    /// <summary>Проверка значения диаграммы направленности диполя на произвольном углу</summary>
    [TestMethod]
    [DataRow(Math.PI / 6, 0.866025)] // 30° -> cos(30°)
    [DataRow(Math.PI / 4, 0.707107)] // 45° -> cos(45°)
    [DataRow(Math.PI / 3, 0.5)]      // 60° -> cos(60°)
    public void Pattern_AtVariousAngles_ShouldReturnCosineValue(double theta, double expected_cos)
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        var direction = new SpaceAngle(theta, 0);
        const double frequency = 1e9;
        
        // Act
        var result = dipole.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(expected_cos, result.Abs, 1e-5, "Диаграмма диполя должна следовать cos(θ)");
    }

    /// <summary>Проверка независимости диаграммы от частоты</summary>
    [TestMethod]
    public void Pattern_ShouldNotDependOnFrequency()
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        var direction = new SpaceAngle(Math.PI / 3, 0);
        
        // Act
        var result1 = dipole.Pattern(direction, 1e9);
        var result2 = dipole.Pattern(direction, 1e10);
        var result3 = dipole.Pattern(direction, 1e11);
        
        // Assert
        Assert.AreEqual(result1, result2, "Диаграмма диполя не должна зависеть от частоты");
        Assert.AreEqual(result2, result3, "Диаграмма диполя не должна зависеть от частоты");
    }

    /// <summary>Проверка симметрии диаграммы относительно азимута</summary>
    [TestMethod]
    public void Pattern_ShouldBeSymmetricalInPhi()
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        var direction1 = new SpaceAngle(Math.PI / 4, 0);
        var direction2 = new SpaceAngle(Math.PI / 4, Math.PI / 2);
        var direction3 = new SpaceAngle(Math.PI / 4, Math.PI);
        const double frequency = 1e9;
        
        // Act
        var result1 = dipole.Pattern(direction1, frequency);
        var result2 = dipole.Pattern(direction2, frequency);
        var result3 = dipole.Pattern(direction3, frequency);
        
        // Assert
        Assert.AreEqual(result1, result2, "Диаграмма диполя не должна зависеть от азимута");
        Assert.AreEqual(result2, result3, "Диаграмма диполя не должна зависеть от азимута");
    }

    /// <summary>Проверка ToString</summary>
    [TestMethod]
    public void ToString_ShouldReturnDipoleName()
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        
        // Act
        var name = dipole.ToString();
        
        // Assert
        Assert.AreEqual("Диполь", name);
    }
}
