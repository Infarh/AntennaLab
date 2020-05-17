using System;
using System.Collections.Generic;
using System.Linq;
using MathCore.Vectors;
using MathCore.WPF.ViewModels;

namespace ArrayFactor.Service.AmplitudeDistributions
{
    /// <summary>Амплитудное распределение</summary>
    public abstract class Distribution : ViewModel
    {
        /// <summary>Значение амплитудного распределения в указанной точке нормированной апертуры</summary>
        /// <param name="x">Нормированное значение координаты <paramref name="x"/></param>
        /// <param name="y">Нормированное значение координаты <paramref name="y"/></param>
        /// <param name="z">Нормированное значение координаты <paramref name="x"/></param>
        /// <returns>Значение амплитудного распределения в указанной точке</returns>
        public abstract double Value(double x, double y, double z);
        /// <summary>Значение амплитудного распределения в указанной точке нормированной апертуры</summary>
        /// <param name="r">Вектор, определяющий положение точки в нормированной апертуре</param>
        /// <returns>Значение амплитудного распределения в указанной точке апертуры</returns>
        public double Value(Vector3D r) => Value(r.X, r.Y, r.Z);

        private static IEnumerable<double> GetX(double dx, double x1, double x2)
        {
            if (dx.Equals(0d)) throw new ArgumentException(@"Шаг должен быть отличен от нуля", nameof(dx));
            if (x1.Equals(x2)) throw new InvalidOperationException("Интервал нулевой длины");
            var x = Math.Min(x1, x2);
            dx = Math.Abs(dx);
            x2 = Math.Max(x1, x2);
            while (x < x2)
            {
                yield return x;
                x += dx;
            }
            yield return x2;
        }

        private double Projection(double x, double phi) => Value(x * Math.Cos(phi), x * Math.Sin(phi), 0);

        public DistributionValue[] GetDistribution(double phi = 0, double dx = 0.01, double x1 = -0.5, double x2 = 0.5) => GetX(dx, x1, x2)
            .AsParallel()
            .AsOrdered()
            .Select(x => new DistributionValue(x, Projection(x, phi)))
            .ToArray();
    }

    public readonly struct DistributionValue
    {
        public double X { get; }
        public double A { get; }
        public double Adb => 20 * Math.Log10(Math.Abs(A));

        public DistributionValue(double x, double a)
        {
            X = x;
            A = a;
        }
    }
}