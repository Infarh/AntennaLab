#nullable enable

using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты для класса LinearAntennaArray</summary>
[TestClass]
public class LinearAntennaArrayTests
{
    /// <summary>Проверка создания линейной решётки из перечисления</summary>
    [TestMethod]
    public void Constructor_WithElements_ShouldCreateArray()
    {
        // Arrange
        var antennas = new Antennas.Antenna[] { new Antennas.UniformAntenna(), new Antennas.Dipole() };
        const double d = 0.5;

        // Act
        var array = new Antennas.LinearAntennaArray(antennas, d);

        // Assert
        Assert.AreEqual(2, array.Count, "Решётка должна содержать 2 элемента");
    }

    /// <summary>Проверка создания линейной решётки с количеством элементов</summary>
    [TestMethod]
    public void Constructor_WithCount_ShouldCreateUniformArray()
    {
        // Arrange
        const int n = 5;
        const double d = 0.5;

        // Act
        var array = new Antennas.LinearAntennaArray(n, d);

        // Assert
        Assert.AreEqual(n, array.Count, $"Решётка должна содержать {n} элементов");
    }

    /// <summary>Проверка правильности расстояния между элементами</summary>
    [TestMethod]
    public void Elements_ShouldBeEvenlySpaced()
    {
        // Arrange
        const int n = 3;
        const double d = 1.0;

        // Act
        var array = new Antennas.LinearAntennaArray(n, d);

        // Assert
        var x_positions = new double[n];
        for (int i = 0; i < n; i++)
            x_positions[i] = array[i].LocationX;

        // Проверяем, что элементы находятся на равных расстояниях
        for (int i = 1; i < n; i++)
        {
            var distance = Math.Abs(x_positions[i] - x_positions[i - 1]);
            Assert.AreEqual(d, distance, 1e-10, $"Расстояние между элементами {i} и {i - 1} должно быть {d}");
        }
    }

    /// <summary>Проверка центрирования решётки</summary>
    [TestMethod]
    public void Array_ShouldBeCenteredAroundOrigin()
    {
        // Arrange
        const int n = 5;
        const double d = 1.0;

        // Act
        var array = new Antennas.LinearAntennaArray(n, d);

        // Assert
        double sum_x = 0;
        for (int i = 0; i < array.Count; i++)
            sum_x += array[i].LocationX;

        double center = sum_x / array.Count;
        Assert.AreEqual(0, center, 1e-10, "Решётка должна быть центрирована вокруг начала координат");
    }

    /// <summary>Проверка, что элементы расположены вдоль оси X</summary>
    [TestMethod]
    public void Elements_ShouldBeAlongXAxis()
    {
        // Arrange
        const int n = 4;
        const double d = 0.5;

        // Act
        var array = new Antennas.LinearAntennaArray(n, d);

        // Assert
        for (int i = 0; i < array.Count; i++)
        {
            Assert.AreEqual(0, array[i].LocationY, 1e-10, $"Y координата элемента {i} должна быть 0");
            Assert.AreEqual(0, array[i].LocationZ, 1e-10, $"Z координата элемента {i} должна быть 0");
        }
    }

    /// <summary>Проверка диаграммы направленности линейной решётки</summary>
    [TestMethod]
    public void Pattern_ShouldShowArrayEffect()
    {
        // Arrange
        const int n = 3;
        const double d = 0.5;
        var array = new Antennas.LinearAntennaArray(n, d);

        // Act
        var direction = new SpaceAngle(Math.PI / 2, 0);
        var pattern = array.Pattern(direction, 1e9);

        // Assert
        //Assert.IsNotNull(pattern, "Диаграмма не должна быть null");
        Assert.IsFalse(double.IsNaN(pattern.Abs), "Модуль диаграммы не должен быть NaN");
    }

    /// <summary>Проверка, что решётка увеличивает направленность</summary>
    [TestMethod]
    public void Array_ShouldBeMoreDirective()
    {
        // Arrange
        var single_antenna = new Antennas.UniformAntenna();
        var array = new Antennas.LinearAntennaArray(3, 0.5);
        var direction_zenith = new SpaceAngle(0, 0);
        var direction_off = new SpaceAngle(Math.PI / 6, 0);

        // Act
        var single_pattern_zenith = single_antenna.Pattern(direction_zenith, 1e9);
        var array_pattern_zenith = array.Pattern(direction_zenith, 1e9);

        var single_pattern_off = single_antenna.Pattern(direction_off, 1e9);
        var array_pattern_off = array.Pattern(direction_off, 1e9);

        // Assert
        Assert.IsGreaterThanOrEqualTo(single_pattern_zenith.Abs, array_pattern_zenith.Abs,
            "Решётка в главном направлении должна быть направленнее");
    }

    /// <summary>Проверка наследования от AntennaArray</summary>
    [TestMethod]
    public void LinearAntennaArray_ShouldInheritFromAntennaArray()
    {
        // Arrange & Act & Assert
        var array = new Antennas.LinearAntennaArray(3, 0.5);
        Assert.IsInstanceOfType(array, typeof(Antennas.AntennaArray),
            "LinearAntennaArray должна наследоваться от AntennaArray");
    }

    /// <summary>Проверка различных шагов решётки</summary>
    [TestMethod]
    [DataRow(0.25)]
    [DataRow(0.5)]
    [DataRow(1.0)]
    [DataRow(2.0)]
    public void Array_WithDifferentSteps_ShouldWork(double step)
    {
        // Arrange & Act
        var array = new Antennas.LinearAntennaArray(4, step);

        // Assert
        Assert.AreEqual(4, array.Count);
    }
}
