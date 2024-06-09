namespace ArrayFactor.Service.AmplitudeDistributions;

public class Lambda(Func<double, double, double, double> A) : Distribution
{
    public override double Value(double x, double y, double z) => A(x, y, z);
}