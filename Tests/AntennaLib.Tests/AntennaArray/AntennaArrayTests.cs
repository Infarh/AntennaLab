#nullable enable

using System.Globalization;

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты базовой антенной решётки</summary>
[TestClass]
public class AntennaArrayTests
{
    private static AntennaItem CreateItem(double x = 0) =>
        new(new UniformAntenna(), new Vector3D(x), SpaceAngle.k, Complex.Real);

    /// <summary>AddRange должен добавлять элементы к непустой решётке</summary>
    [TestMethod]
    public void AddRange_ToNonEmptyArray_AddsAllItems()
    {
        var array = new AntennaArray([CreateItem()]);

        array.AddRange([CreateItem(1), CreateItem(2)]);

        Assert.AreEqual(3, array.Count);
    }

    /// <summary>AddRange должен работать на пустой решётке</summary>
    [TestMethod]
    public void AddRange_ToEmptyArray_AddsAllItems()
    {
        var array = new AntennaArray([]);

        array.AddRange([CreateItem(), CreateItem(1)]);

        Assert.AreEqual(2, array.Count);
    }

    /// <summary>AddRange не должен принимать null-коллекцию</summary>
    [TestMethod]
    public void AddRange_NullCollection_ThrowsArgumentNullException() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new AntennaArray([]).AddRange(null!));

    /// <summary>AddRange не должен принимать null-элемент</summary>
    [TestMethod]
    public void AddRange_WithNullItem_ThrowsArgumentNullException() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new AntennaArray([]).AddRange([null!]));

    /// <summary>Add не должен принимать null-элемент</summary>
    [TestMethod]
    public void Add_Null_ThrowsArgumentNullException() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new AntennaArray([]).Add(null));

    /// <summary>Insert не должен принимать null-элемент</summary>
    [TestMethod]
    public void Insert_Null_ThrowsArgumentNullException() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new AntennaArray([]).Insert(0, null));

    /// <summary>ДН пустой решётки должна быть NaN</summary>
    [TestMethod]
    public void Pattern_EmptyArray_ReturnsNaN()
    {
        var array = new AntennaArray([]);

        var pattern = array.Pattern(new SpaceAngle(), 1e9);

        Assert.IsTrue(double.IsNaN(pattern.Re));
        Assert.IsTrue(double.IsNaN(pattern.Im));
    }

    /// <summary>Плоская решётка должна быть центрирована относительно начала координат</summary>
    [TestMethod]
    public void CreateFlatArray_ShouldBeCentered()
    {
        var antennas = new Antenna[3, 3];
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                antennas[i, j] = new UniformAntenna();

        var array = AntennaArray.CreateFlatArray(antennas, 1, 1);

        Assert.AreEqual(0, array.Average(i => i.LocationX), 1e-12);
        Assert.AreEqual(0, array.Average(i => i.LocationY), 1e-12);
        Assert.AreEqual(2, array.L_x, 1e-12);
        Assert.AreEqual(2, array.L_y, 1e-12);
    }

    /// <summary>Сохранение в файл должно создавать каталог и использовать инвариантную культуру</summary>
    [TestMethod]
    public void SaveToFile_CreatesDirectoryAndUsesInvariantCulture()
    {
        var old_culture = CultureInfo.CurrentCulture;
        var directory = Path.Combine(Path.GetTempPath(), "AntennaLabTests", Guid.NewGuid().ToString("N"));
        var file = Path.Combine(directory, "array.tsv");
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            var array = new AntennaArray([CreateItem(0.125)]);

            array.SaveToFile(file);

            Assert.IsTrue(File.Exists(file));
            var text = File.ReadAllText(file);
            StringAssert.Contains(text, "0.125");
        }
        finally
        {
            CultureInfo.CurrentCulture = old_culture;
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    /// <summary>Выражение ДН пустой решётки должно компилироваться и возвращать NaN</summary>
    [TestMethod]
    public void GetPatternExpression_EmptyArray_ReturnsNaN()
    {
        var array = new AntennaArray([]);

        var compiled = array.GetPatternExpression().Compile();
        var value = compiled(new SpaceAngle(), 1e9);

        Assert.IsTrue(double.IsNaN(value.Re));
    }
}
