#nullable enable

using MathCore;

namespace Antennas.Tests;

/// <summary>Модульные тесты расширений расчёта сечений ДН</summary>
[TestClass]
public class AntennaExtensionsTests
{
    /// <summary>Конечная точка диапазона должна достигаться при неделящемся шаге</summary>
    [TestMethod]
    public void GetPatternPhi_NonDivisibleStep_ReachesEndAngle()
    {
        var antenna = new UniformAntenna();

        var pattern = antenna.GetPatternPhi(1e9, 0, -1, 1, 0.3);

        Assert.AreEqual(1, pattern[^1].Angle, 1e-12);
        Assert.HasCount(8, pattern);
    }

    /// <summary>Нулевой шаг должен приводить к явной ошибке</summary>
    [TestMethod]
    public void GetPatternPhi_ZeroStep_ThrowsArgumentOutOfRangeException() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new UniformAntenna().GetPatternPhi(1e9, 0, -1, 1, 0));

    /// <summary>Отрицательный шаг должен приводить к явной ошибке</summary>
    [TestMethod]
    public void GetPatternValuesParallel_NegativeStep_ThrowsArgumentOutOfRangeException() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new UniformAntenna().GetPatternValuesParallel(1e9, 0, -1, 1, -0.1));

    /// <summary>Параллельный расчёт должен совпадать с последовательным</summary>
    [TestMethod]
    public void GetPatternValuesParallel_MatchesSequential()
    {
        var antenna = new LinearAntennaArray(8, 0.15);

        var sequential = antenna.GetPatternPhi(1e9, 0, -90 * Consts.ToRad, 90 * Consts.ToRad, 1 * Consts.ToRad);
        var parallel = antenna.GetPatternValuesParallel(1e9, 0, -90 * Consts.ToRad, 90 * Consts.ToRad, 1 * Consts.ToRad);

        Assert.HasCount(sequential.Length, parallel);
        for (var i = 0; i < sequential.Length; i++)
        {
            Assert.AreEqual(sequential[i].Angle, parallel[i].Angle, 1e-12);
            Assert.AreEqual(sequential[i].Value.Re, parallel[i].Value.Re, 1e-12);
            Assert.AreEqual(sequential[i].Value.Im, parallel[i].Value.Im, 1e-12);
        }
    }

    /// <summary>Асинхронный расчёт должен сообщать прогресс до 1</summary>
    [TestMethod]
    public async Task GetPatternPhiAsync_ReportsProgressUpTo1()
    {
        var progress = new SynchronousProgress<PatternCalculationTaskProgressInfo>();

        await new UniformAntenna().GetPatternPhiAsync(1e9, 0, -1, 1, 0.5, progress);

        Assert.AreEqual(1, progress.Last.Progress, 1e-12);
    }

    /// <summary>Отмена должна прерывать расчёт</summary>
    [TestMethod]
    public async Task GetPatternPhiAsync_Cancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() =>
            new UniformAntenna().GetPatternPhiAsync(1e9, 0, -1, 1, 0.001, null, cts.Token));
    }

    private sealed class SynchronousProgress<T> : IProgress<T>
    {
        public T? Last { get; private set; }

        public void Report(T value) => Last = value;
    }
}
