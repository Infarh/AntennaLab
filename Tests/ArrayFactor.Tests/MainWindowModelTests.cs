#nullable enable

using System.Reflection;

namespace ArrayFactor.Tests;

/// <summary>Интеграционные тесты расчётного конвейера главной модели</summary>
[TestClass]
public class MainWindowModelTests
{
    private static Task RunComputePatternAsync(MainWindowModel Model)
    {
        var method = typeof(MainWindowModel).GetMethod(
            "ComputePatternCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, "Не найден метод расчёта ДН");

        return (Task)method.Invoke(Model, null)!;
    }

    /// <summary>После расчёта сечения ДН должны быть заполнены, а КНД должен быть близок к 12 дБ</summary>
    [TestMethod]
    public async Task ComputePattern_FillsBeamDataAndDirectivity()
    {
        var model = new MainWindowModel
        {
            Nx = 16,
            Ny = 1,
            dx = 15,
            dy = 15,
            f0 = 1,
            θ0 = 0,
            φ0 = 0,
            PhiView = 0
        };

        await RunComputePatternAsync(model);

        Assert.IsNotNull(model.BeamData);
        Assert.IsNotNull(model.Beam0Data);
        Assert.IsNotEmpty(model.BeamData);
        Assert.IsNotEmpty(model.Beam0Data);
        Assert.HasCount(361, model.BeamData);
        Assert.AreEqual(12.04, model.D0, 0.5);
        Assert.IsTrue(double.IsFinite(model.MaxFieldDb));
        Assert.IsTrue(double.IsFinite(model.BeamWidth07));
    }

    /// <summary>Изменение угла обзора не должно менять амплитудное распределение решётки</summary>
    [TestMethod]
    public async Task PhiViewChange_DoesNotRebuildDistribution()
    {
        var model = new MainWindowModel();
        await RunComputePatternAsync(model);
        var coefficients = model.AntennaArray.Select(i => i.K).ToArray();

        model.PhiView = 45;
        await RunComputePatternAsync(model);

        var new_coefficients = model.AntennaArray.Select(i => i.K).ToArray();
        CollectionAssert.AreEqual(coefficients, new_coefficients);
    }
}
