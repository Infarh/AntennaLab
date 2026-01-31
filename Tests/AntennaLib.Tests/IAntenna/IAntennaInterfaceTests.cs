#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты интерфейса IAntenna</summary>
[TestClass]
public class IAntennaInterfaceTests
{
    /// <summary>Проверка наличия метода Pattern с правильной сигнатурой</summary>
    [TestMethod]
    public void Interface_HasPatternMethod()
    {
        // Arrange & Act & Assert
        var interface_type = typeof(Antennas.IAntenna);
        var pattern_method = interface_type.GetMethod(nameof(Antennas.IAntenna.Pattern), new[] { typeof(SpaceAngle), typeof(double) });
        
        Assert.IsNotNull(pattern_method, "Интерфейс IAntenna должен иметь метод Pattern(SpaceAngle, double)");
        Assert.AreEqual(typeof(Complex), pattern_method!.ReturnType, "Метод Pattern должен возвращать Complex");
    }

    /// <summary>Проверка реализации интерфейса классом Dipole</summary>
    [TestMethod]
    public void DipoleMustImplementInterface()
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        
        // Act & Assert
        Assert.IsInstanceOfType(dipole, typeof(Antennas.IAntenna), "Класс Dipole должен реализовывать интерфейс IAntenna");
    }

    /// <summary>Проверка реализации интерфейса классом UniformAntenna</summary>
    [TestMethod]
    public void UniformAntennaMustImplementInterface()
    {
        // Arrange
        var antenna = new Antennas.UniformAntenna();
        
        // Act & Assert
        Assert.IsInstanceOfType(antenna, typeof(Antennas.IAntenna), "Класс UniformAntenna должен реализовывать интерфейс IAntenna");
    }
}
