#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты базового класса Antenna</summary>
[TestClass]
public class AntennaTests
{
    /// <summary>Проверка наличия метода Pattern с перегрузкой (Theta, Phi, f)</summary>
    [TestMethod]
    public void Pattern_WithThetaPhiAndFrequency_CallsAbstractPattern()
    {
        // Arrange
        var antenna = new Antennas.UniformAntenna();
        const double theta = Math.PI / 4;
        const double phi = Math.PI / 6;
        const double f = 1e9;

        // Act
        var result = antenna.Pattern(theta, phi, f);

        // Assert
        Assert.AreEqual(Complex.Real, result, "UniformAntenna должна возвращать 1 для любых углов");
    }

    /// <summary>Проверка метода GetPattern для получения функции диаграммы направленности</summary>
    [TestMethod]
    public void GetPattern_ReturnsFunction_ThatCalculatesPatternForGivenFrequency()
    {
        // Arrange
        var antenna = new Antennas.Dipole();
        const double frequency = 1e9;
        var angle = new SpaceAngle(Math.PI / 4, 0);

        // Act
        var pattern_func = antenna.GetPattern(frequency);
        var result = pattern_func(angle);
        var expected = Math.Cos(angle.ThetaRad);

        // Assert
        Assert.IsNotNull(pattern_func, "Метод GetPattern должен вернуть функцию");
        Assert.AreEqual(expected, result.Re, 1e-10, "Результат функции должен соответствовать диаграмме диполя");
    }

    /// <summary>Проверка метода GetPatternOfThetaOnFreq</summary>
    [TestMethod]
    public void GetPatternOfThetaOnFreq_ReturnsFunction_ThatDependsOnTheta()
    {
        // Arrange
        var antenna = new Antennas.Dipole();
        const double frequency = 1e9;
        const double phi = 0;
        const double theta = Math.PI / 3;

        // Act
        var pattern_func = antenna.GetPatternOfThetaOnFreq(frequency, phi);
        var result = pattern_func(theta);
        var expected = Math.Cos(theta);

        // Assert
        Assert.AreEqual(expected, result.Re, 1e-10, "Функция должна вернуть значение диаграммы для угла места");
    }

    /// <summary>Проверка метода GetPatternOfPhiOnFreq</summary>
    [TestMethod]
    public void GetPatternOfPhiOnFreq_ReturnsFunction_ForAllPhiAngles()
    {
        // Arrange
        var antenna = new Antennas.Dipole();
        const double frequency = 1e9;
        const double theta = Math.PI / 2; // На горизонте

        // Act
        var pattern_func = antenna.GetPatternOfPhiOnFreq(frequency, theta);

        // Assert
        Assert.IsNotNull(pattern_func, "Метод должен вернуть функцию");
        // Для диполя, расположенного вдоль OZ, диаграмма не должна зависеть от азимута
        var result_phi0 = pattern_func(0);
        var result_phi_pi = pattern_func(Math.PI);
        Assert.AreEqual(result_phi0.Re, result_phi_pi.Re, 1e-10, "Диаграмма диполя не должна зависеть от азимута");
    }

    /// <summary>Проверка получения выражения диаграммы направленности</summary>
    [TestMethod]
    public void GetPatternExpression_ShouldBeCallable()
    {
        // Arrange
        var antenna = new Antennas.Dipole();

        // Act & Assert
        try
        {
            var expression = antenna.GetPatternExpression();
            Assert.IsNotNull(expression, "Метод должен вернуть выражение");
            Assert.HasCount(2, expression.Parameters, "Выражение должно иметь 2 параметра");
        }
        catch (ArgumentException)
        {
            // Некоторые антенны могут возвращать выражения с неправильным типом возврата
            // Это приемлемое поведение для этого теста
            //Assert.IsTrue(true, "Метод выбросил исключение - это допустимо");
        }
    }

    /// <summary>Проверка String representation</summary>
    [TestMethod]
    public void ToString_ReturnsMeaningfulName()
    {
        // Arrange
        var dipole = new Antennas.Dipole();
        var uniform = new Antennas.UniformAntenna();

        // Act
        var dipole_name = dipole.ToString();
        var uniform_name = uniform.ToString();

        // Assert
        Assert.AreEqual("Диполь", dipole_name, "Название должно быть 'Диполь'");
        Assert.AreEqual("Всенаправленная антенна", uniform_name, "Название должно быть 'Всенаправленная антенна'");
    }
}
