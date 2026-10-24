using static System.Math;

namespace ArrayFactor.Service.AmplitudeDistributions;

/// <summary>Гауссово амплитудное распределение</summary>
/// <remarks>Параметры <see cref="Dx"/> и <see cref="Dy"/> задают уровень поля на краю апертуры</remarks>
public class Gaussian : Distribution
{
    private double _Dx = 0.1;
    private double _Dy = 0.1;

    /// <summary>Уровень поля на краю апертуры по оси OX</summary>
    public double Dx { get => _Dx; set => Set(ref _Dx, value, D => D is >= 0 and <= 1); }

    /// <summary>Уровень поля на краю апертуры по оси OY</summary>
    public double Dy { get => _Dy; set => Set(ref _Dy, value, D => D is >= 0 and <= 1); }

    private static double A(double x, double D) => Pow(D, 4 * x * x);

    /// <inheritdoc />
    public override double Value(double x, double y, double z) => A(x, _Dx) * A(y, _Dy);

    /// <inheritdoc />
    public override string ToString() => "Гауссово";
}
