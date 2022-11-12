#nullable enable
namespace ArrayFactor.Service.AmplitudeDistributions;

/// <summary>Чебышевское распределение</summary>
public class Chebishew : Distribution
{
    private double _Dx = 0.7;
    private double _Dy = 0.7;

    public double Dx { get => _Dx; set => Set(ref _Dx, value, D => D is >= 0 and <= 1); }
    public double Dy { get => _Dy; set => Set(ref _Dy, value, D => D is >= 0 and <= 1); }

    private static double A(double x, double D) => 1 - (1 - D) * x * x * 4;

    /// <inheritdoc />
    public override double Value(double x, double y, double z) => A(x, _Dx) * A(y, _Dy);
}