#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты прямоугольной плоской решётки</summary>
[TestClass]
public class RectangularAntennaArrayTests
{
    private static RectangularAntennaArray CreateArray(int nx = 4, int ny = 2) =>
        new(nx, ny, 0.1, 0.2, new UniformAntenna(), (_, _) => Complex.Real);

    /// <summary>Изменение числа элементов должно перестраивать решётку</summary>
    [TestMethod]
    public void NxAndNy_Changes_RebuildArray()
    {
        var array = CreateArray();

        array.Nx = 5;
        array.Ny = 3;

        Assert.AreEqual(5, array.Nx);
        Assert.AreEqual(3, array.Ny);
        Assert.AreEqual(15, array.Count);
    }

    /// <summary>Изменение шага должно перестраивать геометрию с сохранением центрирования</summary>
    [TestMethod]
    public void dxAndDy_Changes_RecalculateGeometry()
    {
        var array = CreateArray();

        array.dx = 0.3;
        array.dy = 0.4;

        Assert.AreEqual(0.3 * 3, array.L_x, 1e-12);
        Assert.AreEqual(0.4 * 1, array.L_y, 1e-12);
        Assert.AreEqual(0, array.Average(i => i.LocationX), 1e-12);
        Assert.AreEqual(0, array.Average(i => i.LocationY), 1e-12);
    }

    /// <summary>Распределение должно применяться ко всем элементам</summary>
    [TestMethod]
    public void Distribution_AppliesToAllItems()
    {
        var array = CreateArray();

        array.Distribution = (_, _) => new Complex(0.5, 0.5);

        Assert.IsTrue(array.All(i => Math.Abs(i.K.Re - 0.5) < 1e-12 && Math.Abs(i.K.Im - 0.5) < 1e-12));
    }

    /// <summary>Изменение геометрии после ручного изменения коллекции должно приводить к явной ошибке</summary>
    [TestMethod]
    public void RefreshGeometry_AfterManualCollectionChange_ThrowsInvalidOperationException()
    {
        var array = CreateArray();
        array.Add(new AntennaItem(new UniformAntenna(), Vector3D.Empty, SpaceAngle.k, Complex.Real));

        Assert.ThrowsExactly<InvalidOperationException>(() => array.dx = 0.5);
    }
}
