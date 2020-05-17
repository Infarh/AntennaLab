using System;
using MathCore;
using MathCore.Annotations;

namespace ArrayFactor.Service.AmplitudeDistributions
{
    public class CosOnDelta : Distribution
    {
        private double _Dx = 0.2;
        private double _Dy = 0.2;
        private int _Nx = 2;
        private int _Ny = 2;

        public double Dx { get => _Dx; set => Set(ref _Dx, value, v => v >= 0 && v <= 1); }

        public double Dy { get => _Dy; set => Set(ref _Dy, value, v => v >= 0 && v <= 1); }

        public int Nx { get => _Nx; set => Set(ref _Nx, value, n => n > 0); }
        public int Ny { get => _Ny; set => Set(ref _Ny, value, n => n > 0); }

        private static double A(double x, double D, int n) { var cos = Math.Cos(Consts.pi * x); return D + (1 - D) * Math.Pow(cos, n); }

        public override double Value(double x, double y, double z) => A(x, _Dx, _Nx) * A(y, _Dy, _Ny);

        [NotNull]
        public override string ToString() => $"cos^({_Nx};{_Ny}) Dx{_Dx} Dy{_Dy}";
    }
}