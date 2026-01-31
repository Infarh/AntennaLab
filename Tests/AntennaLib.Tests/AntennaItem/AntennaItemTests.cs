#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты для класса AntennaItem</summary>
[TestClass]
public class AntennaItemTests
{
    /// <summary>Проверка конструктора по умолчанию</summary>
    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var item = new Antennas.AntennaItem();
        
        // Assert
        Assert.IsNotNull(item.Element, "Элемент должен быть инициализирован");
        Assert.IsInstanceOfType(item.Element, typeof(Antennas.UniformAntenna), 
            "По умолчанию должна быть всенаправленная антенна");
        Assert.AreEqual(Vector3D.Empty, item.Location, "Местоположение должно быть пустым");
        Assert.AreEqual(1.0, item.K.Re, 1e-10, "Коэффициент передачи должен быть 1");
    }

    /// <summary>Проверка инициализации с параметрами</summary>
    [TestMethod]
    public void Constructor_WithParameters_ShouldSetProperties()
    {
        // Arrange
        var antenna = new Antennas.Dipole();
        var location = new Vector3D(1, 2, 3);
        var direction = new SpaceAngle(Math.PI / 4, Math.PI / 3);
        var coefficient = new Complex(0.5, 0.5);
        
        // Act
        var item = new Antennas.AntennaItem(antenna, location, direction, coefficient);
        
        // Assert
        Assert.AreSame(antenna, item.Element, "Элемент должен быть тот же");
        Assert.AreEqual(location, item.Location, "Местоположение должно совпадать");
        Assert.AreEqual(coefficient, item.K, "Коэффициент передачи должен совпадать");
    }

    /// <summary>Проверка свойства Element</summary>
    [TestMethod]
    public void Element_CanBeChanged()
    {
        // Arrange
        var item = new Antennas.AntennaItem();
        var new_antenna = new Antennas.Dipole();
        
        // Act
        item.Element = new_antenna;
        
        // Assert
        Assert.AreSame(new_antenna, item.Element, "Элемент должен быть изменён");
    }

    /// <summary>Проверка свойства Location и компонентов</summary>
    [TestMethod]
    public void Location_CanBeModified()
    {
        // Arrange
        var item = new Antennas.AntennaItem();
        var new_location = new Vector3D(5, 10, 15);
        
        // Act
        item.Location = new_location;
        
        // Assert
        Assert.AreEqual(5, item.LocationX, 1e-10);
        Assert.AreEqual(10, item.LocationY, 1e-10);
        Assert.AreEqual(15, item.LocationZ, 1e-10);
    }

    /// <summary>Проверка изменения компонентов через свойства</summary>
    [TestMethod]
    public void LocationComponents_CanBeChangedIndividually()
    {
        // Arrange
        var item = new Antennas.AntennaItem();
        
        // Act
        item.LocationX = 1;
        item.LocationY = 2;
        item.LocationZ = 3;
        
        // Assert
        Assert.AreEqual(1, item.LocationX, 1e-10);
        Assert.AreEqual(2, item.LocationY, 1e-10);
        Assert.AreEqual(3, item.LocationZ, 1e-10);
    }

    /// <summary>Проверка свойства Direction</summary>
    [TestMethod]
    public void Direction_CanBeChanged()
    {
        // Arrange
        var item = new Antennas.AntennaItem();
        var new_direction = new SpaceAngle(Math.PI / 3, Math.PI / 6);
        
        // Act
        item.Direction = new_direction;
        
        // Assert
        Assert.AreEqual(new_direction.Theta, item.Theta, 1e-10);
        Assert.AreEqual(new_direction.Phi, item.Phi, 1e-10);
    }

    /// <summary>Проверка углов в радианах и градусах</summary>
    [TestMethod]
    public void AngleDegrees_ConvertProperly()
    {
        // Arrange
        var item = new Antennas.AntennaItem();
        const double theta_deg = 90;
        const double phi_deg = 45;
        
        // Act
        item.ThetaDeg = theta_deg;
        item.PhiDeg = phi_deg;
        
        // Assert
        Assert.AreEqual(theta_deg, item.ThetaDeg, 1e-10);
        Assert.AreEqual(phi_deg, item.PhiDeg, 1e-10);
        Assert.AreEqual(Math.PI / 2, item.Theta, 1e-10);
        Assert.AreEqual(Math.PI / 4, item.Phi, 1e-10);
    }

    /// <summary>Проверка комплексного коэффициента передачи</summary>
    [TestMethod]
    public void ComplexCoefficient_CanBeModified()
    {
        // Arrange
        var item = new Antennas.AntennaItem();
        var new_coefficient = new Complex(0.7, 0.3);
        
        // Act
        item.K = new_coefficient;
        
        // Assert
        Assert.AreEqual(0.7, item.ReK, 1e-10);
        Assert.AreEqual(0.3, item.ImK, 1e-10);
    }

    /// <summary>Проверка модуля и аргумента коэффициента</summary>
    [TestMethod]
    public void CoefficientModulusAndPhase_CanBeModified()
    {
        // Arrange
        var item = new Antennas.AntennaItem();
        const double abs_value = 0.8;
        const double angle = Math.PI / 4;
        
        // Act
        item.AbsK = abs_value;
        item.ArgK = angle;
        
        // Assert
        Assert.AreEqual(abs_value, item.AbsK, 1e-10);
    }

    /// <summary>Проверка метода Pattern</summary>
    [TestMethod]
    public void Pattern_ShouldReturnPatternOfElement()
    {
        // Arrange
        var antenna = new Antennas.Dipole();
        var item = new Antennas.AntennaItem(antenna, Vector3D.Empty, SpaceAngle.k, 1);
        var direction = new SpaceAngle(Math.PI / 3, 0);
        const double frequency = 1e9;
        
        // Act
        var item_pattern = item.Pattern(direction, frequency);
        var antenna_pattern = antenna.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(antenna_pattern, item_pattern, 
            "Диаграмма элемента должна совпадать с диаграммой антенны");
    }

    /// <summary>Проверка влияния коэффициента передачи на диаграмму</summary>
    [TestMethod]
    public void Pattern_WithCoefficient_ShouldMultiplyByCoefficient()
    {
        // Arrange
        var antenna = new Antennas.Dipole();
        var coefficient = new Complex(0.5, 0);
        var item = new Antennas.AntennaItem(antenna, Vector3D.Empty, SpaceAngle.k, coefficient);
        var direction = new SpaceAngle(Math.PI / 3, 0);
        const double frequency = 1e9;
        
        // Act
        var item_pattern = item.Pattern(direction, frequency);
        var antenna_pattern = antenna.Pattern(direction, frequency);
        
        // Assert
        Assert.AreEqual(antenna_pattern * coefficient, item_pattern, 
            "Диаграмма должна быть умножена на коэффициент");
    }
}
