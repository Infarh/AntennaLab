#nullable enable
namespace ArrayFactor.Service.AmplitudeDistributions;

public class Uniform : Distribution
{
    public override double Value(double x, double y, double z) => 1;

    public override string ToString() => "Равномерное";
}