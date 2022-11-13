using System;

namespace ArrayFactor.Service.AmplitudeDistributions;

public class Lambda : Distribution
{
    private readonly Func<double, double, double, double> _A;

    public Lambda(Func<double, double, double, double> A) => _A = A;

    public override double Value(double x, double y, double z) => _A(x, y, z);
}