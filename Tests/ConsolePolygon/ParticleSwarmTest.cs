using System;
using System.Linq;
using MathCore;

namespace ConsolePolygon
{
    internal static class ParticleSwarmTest
    {
        private static double f0(double x0, double x1) { return 3 + x0 * x0 + x1 * x1; }

        public static void Test()
        {
            Func<double, double> f = x => x * x;
            Func<double, double> q = x => f(x - 3) + 2;
            var swarm = new Swarm1D();
            swarm.Minimize(q, -100, 100, 1000, out var X, out var Y);
            Console.WriteLine("X = {0}; Y = {1}", X, Y);
            Console.ReadLine();
        }
    }



    /// <summary>Рой двумерных частиц</summary>
    public class Swarm2D
    {
        /// <summary>Вес инерции</summary>
        [Hyperlink("http://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=00870279")]
        private const double w = 0.729;

        /// <summary>Коэффициент локального веса</summary>
        private const double c1 = 1.49445; // cognitive/local weight
        /// <summary>Коэффициент глобального веса</summary>
        private const double c2 = 1.49445; // social/global weight

        /// <summary>Частица</summary>
        private class Particle2D
        {
            public double BestValue;
            public double BestX;
            public double BestY;
            public double Value;
            public double X;
            public double Y;

            public Particle2D(Func<double, double, double> Function, Interval IntervalX, Interval IntervalY)
                : this(Function, IntervalX.RandomValue, IntervalY.RandomValue) { }
            private Particle2D(Func<double, double, double> Function, double X, double Y)
                : this(X, Y, Function(X, Y)) { }
            public Particle2D(double X, double Y, double Value) : this(X, Y, Value, X, Y, Value) { }
            public Particle2D(double X, double Y, double Value, double BestX, double BestY, double BestValue)
            {
                this.X = X;
                this.Y = Y;
                this.Value = Value;
                this.BestX = BestX;
                this.BestY = BestY;
                this.BestValue = BestValue;
            }
        }

        private static readonly Random sf_Random = new();

        /// <summary>Размер роя</summary>
        private readonly int f_ParticleCount;

        public Swarm2D(int ParticleCount = 100) { f_ParticleCount = ParticleCount; }

        public void Minimize(Func<double, double, double> func,
            double minX, double maxX, double minY, double maxY,
            int IterationCount,
            out double X, out double Y, out double Value)
        {
            Minimize(func, new Interval(minX, maxX), new Interval(minY, maxY), IterationCount,
                out X, out Y, out Value);
        }

        public void Minimize(Func<double, double, double> func, Interval IntervalX, Interval IntervalY,
            int IterationCount,
            out double X, out double Y, out double Value)
        {
            var swarm = new Particle2D[f_ParticleCount].Initialize(i => new Particle2D(func, IntervalX, IntervalY));
            var start = swarm.GetMin(p => p.Value);
            X = start.X;
            Y = start.Y;
            Value = start.Value;

            for(var iteration = 0; iteration < IterationCount; iteration++)
                foreach(var p in swarm)
                {
                    var r1 = sf_Random.NextDouble();
                    var r2 = sf_Random.NextDouble();

                    var newVx = (w * p.X) + (c1 * r1 * (p.BestX - p.X)) + (c2 * r2 * (X - p.X));
                    IntervalX.Normalize(ref newVx);

                    r1 = sf_Random.NextDouble();
                    r2 = sf_Random.NextDouble();

                    var newVy = (w * p.Y) + (c1 * r1 * (p.BestY - p.Y)) + (c2 * r2 * (Y - p.Y));
                    IntervalY.Normalize(ref newVy);

                    var newX = IntervalX.Normalize(p.X + newVx);
                    var newY = IntervalX.Normalize(p.Y + newVy);
                    p.X = newX;
                    p.Y = newY;
                    p.Value = func(newX, newY);
                    if(p.Value < p.BestValue)
                    {
                        p.BestX = newX;
                        p.BestY = newY;
                        p.BestValue = p.Value;
                    }
                    if(!(p.Value < Value)) continue;
                    X = newX;
                    Y = newY;
                    Value = p.Value;
                }
        }

        public void Maximize(Func<double, double, double> func,
            double minX, double maxX, double minY, double maxY,
            int IterationCount,
            out double X, out double Y, out double Value)
        {
            Maximize(func, new Interval(minX, maxX), new Interval(minY, maxY), IterationCount,
                out X, out Y, out Value);
        }

        public void Maximize(Func<double, double, double> func, Interval IntervalX, Interval IntervalY,
            int IterationCount,
            out double X, out double Y, out double Value)
        {
            var swarm = new Particle2D[f_ParticleCount].Initialize(i => new Particle2D(func, IntervalX, IntervalY));
            var start = swarm.GetMax(p => p.Value);
            X = start.X;
            Y = start.Y;
            Value = start.Value;

            for(var iteration = 0; iteration < IterationCount; iteration++)
                foreach(var p in swarm)
                {
                    var r1 = sf_Random.NextDouble();
                    var r2 = sf_Random.NextDouble();

                    var newVx = (w * p.X) + (c1 * r1 * (p.BestX - p.X)) + (c2 * r2 * (X - p.X));
                    IntervalX.Normalize(ref newVx);

                    r1 = sf_Random.NextDouble();
                    r2 = sf_Random.NextDouble();

                    var newVy = (w * p.Y) + (c1 * r1 * (p.BestY - p.Y)) + (c2 * r2 * (Y - p.Y));
                    IntervalY.Normalize(ref newVy);

                    var newX = IntervalX.Normalize(p.X + newVx);
                    var newY = IntervalX.Normalize(p.Y + newVy);
                    p.X = newX;
                    p.Y = newY;
                    p.Value = func(newX, newY);
                    if(p.Value > p.BestValue)
                    {
                        p.BestX = newX;
                        p.BestY = newY;
                        p.BestValue = p.Value;
                    }
                    if(!(p.Value > Value)) continue;
                    X = newX;
                    Y = newY;
                    Value = p.Value;
                }
        }
    }

    public class Swarm1D
    {
        private const double w = 0.729; // inertia weight. see http://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=00870279
        private const double c1 = 1.49445; // cognitive/local weight
        private const double c2 = 1.49445; // social/global weight

        private class Particle1D
        {
            public double BestValue;
            public double BestX;
            public double Value;
            public double X;

            public Particle1D(Func<double, double> Function, Interval IntervalX)
                : this(Function, IntervalX.RandomValue) { }
            private Particle1D(Func<double, double> Function, double X)
                : this(X, Function(X)) { }
            public Particle1D(double X, double Value) : this(X, Value, X, Value) { }
            public Particle1D(double X, double Value, double BestX, double BestValue)
            {
                this.X = X;
                this.Value = Value;
                this.BestX = BestX;
                this.BestValue = BestValue;
            }
        }

        private static readonly Random sf_Random = new();

        private readonly int f_ParticleCount;

        public Swarm1D(int ParticleCount = 100) { f_ParticleCount = ParticleCount; }

        public void Minimize(Func<double, double> func, double minX, double maxX, int IterationCount,
            out double X, out double Value)
        {
            Minimize(func, new Interval(minX, maxX), IterationCount, out X, out Value);
        }

        public void Minimize(Func<double, double> func, Interval IntervalX, int IterationCount,
            out double X, out double Value)
        {
            var swarm = new Particle1D[f_ParticleCount].Initialize(i => new Particle1D(func, IntervalX));
            var start = swarm.GetMin(p => p.Value);
            X = start.X;
            Value = start.Value;

            for(var iteration = 0; iteration < IterationCount; iteration++)
                foreach(var p in swarm)
                {
                    var r1 = sf_Random.NextDouble();
                    var r2 = sf_Random.NextDouble();

                    var newVx = (w * p.X) + (c1 * r1 * (p.BestX - p.X)) + (c2 * r2 * (X - p.X));
                    IntervalX.Normalize(ref newVx);

                    var newX = IntervalX.Normalize(p.X + newVx);
                    p.X = newX;
                    p.Value = func(newX);
                    if(p.Value < p.BestValue)
                    {
                        p.BestX = newX;
                        p.BestValue = p.Value;
                    }

                    if(!(p.Value < Value)) continue;
                    X = newX;
                    Value = p.Value;
                }
        }

        public void Maximize(Func<double, double> func, double minX, double maxX, int IterationCount,
            out double X, out double Value)
        {
            Maximize(func, new Interval(minX, maxX), IterationCount, out X, out Value);
        }

        public void Maximize(Func<double, double> func, Interval IntervalX, int IterationCount,
            out double X, out double Value)
        {
            var swarm = new Particle1D[f_ParticleCount].Initialize(i => new Particle1D(func, IntervalX));
            var start = swarm.GetMax(p => p.Value);
            X = start.X;
            Value = start.Value;

            for(var iteration = 0; iteration < IterationCount; iteration++)
                foreach(var p in swarm)
                {
                    var r1 = sf_Random.NextDouble();
                    var r2 = sf_Random.NextDouble();

                    var newVx = (w * p.X) + (c1 * r1 * (p.BestX - p.X)) + (c2 * r2 * (X - p.X));
                    IntervalX.Normalize(ref newVx);

                    var newX = IntervalX.Normalize(p.X + newVx);
                    p.X = newX;
                    p.Value = func(newX);
                    if(p.Value > p.BestValue)
                    {
                        p.BestX = newX;
                        p.BestValue = p.Value;
                    }
                    if(!(p.Value > Value)) continue;
                    X = newX;
                    Value = p.Value;
                }
        }
    }

    public class Swarm
    {
        private const double w = 0.729; // inertia weight. see http://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=00870279
        private const double c1 = 1.49445; // cognitive/local weight
        private const double c2 = 1.49445; // social/global weight

        private class Particle
        {
            public double BestValue;
            public double[] BestX;
            public double Value;
            public double[] X;

            public Particle(Func<double[], double> Function, Interval[] IntervalX)
                : this(Function, IntervalX.Select(i => i.RandomValue).ToArray()) { }
            private Particle(Func<double[], double> Function, double[] X)
                : this(X, Function(X)) { }
            public Particle(double[] X, double Value) : this(X, Value, X, Value) { }
            public Particle(double[] X, double Value, double[] BestX, double BestValue)
            {
                this.X = X;
                this.Value = Value;
                this.BestX = BestX;
                this.BestValue = BestValue;
            }
        }

        private static readonly Random sf_Random = new();

        private readonly int f_ParticleCount;

        public Swarm(int ParticleCount = 100) { f_ParticleCount = ParticleCount; }

        public void Minimize(Func<double[], double> func, double[] minX, double[] maxX, int IterationCount,
            out double[] X, out double Value)
        {
            Minimize(func, minX.Zip(maxX, (min, max) => new Interval(min, max)).ToArray(),
                IterationCount, out X, out Value);
        }

        public void Minimize(Func<double[], double> func, Interval[] IntervalX, int IterationCount,
            out double[] X, out double Value)
        {
            var IntervalVx = new Interval[IntervalX.Length].Initialize(i => IntervalX[i]);

            var swarm = new Particle[f_ParticleCount].Initialize(i => new Particle(func, IntervalX));
            var start = swarm.GetMin(p => p.Value);
            X = start.X;
            Value = start.Value;

            for(var iteration = 0; iteration < IterationCount; iteration++)
                foreach(var p in swarm)
                {
                    var r1 = sf_Random.NextDouble();
                    var r2 = sf_Random.NextDouble();

                    var newVx = p.X.Zip(p.BestX, (x, BestX) => new { x, BestX })
                        .Zip(X, (v, GlobalBestX) => new { v.x, v.BestX, GlobalBestX })
                        .Zip(IntervalVx, (v, I) => new { v.x, v.BestX, v.GlobalBestX, I })
                        .Select(v => new { value = (w * v.x) + (c1 * r1 * (v.BestX - v.x)) + (c2 * r2 * (v.GlobalBestX - v.x)), v.I })
                        .Select(v => v.I.Normalize(v.value))
                        .ToArray();

                    var newX = p.X.Zip(IntervalX, (x, I) => new { x, I })
                                .Zip(newVx, (v, vx) => v.I.Normalize(v.x + vx)).ToArray();
                    p.X = newX;
                    p.Value = func(newX);
                    if(p.Value < p.BestValue)
                    {
                        p.BestX = newX;
                        p.BestValue = p.Value;
                    }

                    if(!(p.Value < Value)) continue;
                    X = newX;
                    Value = p.Value;
                }
        }

        public void Maximize(Func<double[], double> func, double[] minX, double[] maxX, int IterationCount,
            out double[] X, out double Value)
        {
            Maximize(func, minX.Zip(maxX, (min, max) => new Interval(min, max)).ToArray(),
                IterationCount, out X, out Value);
        }

        public void Maximize(Func<double[], double> func, Interval[] IntervalX, int IterationCount,
            out double[] X, out double Value)
        {
            var IntervalVx = new Interval[IntervalX.Length].Initialize(i => IntervalX[i]);

            var swarm = new Particle[f_ParticleCount].Initialize(i => new Particle(func, IntervalX));
            var start = swarm.GetMax(p => p.Value);
            X = start.X;
            Value = start.Value;

            for(var iteration = 0; iteration < IterationCount; iteration++)
                foreach(var p in swarm)
                {
                    var r1 = sf_Random.NextDouble();
                    var r2 = sf_Random.NextDouble();

                    var newVx = p.X.Zip(p.BestX, (x, BestX) => new { x, BestX })
                        .Zip(X, (v, GlobalBestX) => new { v.x, v.BestX, GlobalBestX })
                        .Zip(IntervalVx, (v, I) => new { v.x, v.BestX, v.GlobalBestX, I })
                        .Select(v => new { value = (w * v.x) + (c1 * r1 * (v.BestX - v.x)) + (c2 * r2 * (v.GlobalBestX - v.x)), v.I })
                        .Select(v => v.I.Normalize(v.value))
                        .ToArray();

                    var newX = p.X.Zip(IntervalX, (x, I) => new { x, I })
                                .Zip(newVx, (v, vx) => v.I.Normalize(v.x + vx)).ToArray();
                    p.X = newX;
                    p.Value = func(newX);
                    if(p.Value > p.BestValue)
                    {
                        p.BestX = newX;
                        p.BestValue = p.Value;
                    }
                    if(!(p.Value > Value)) continue;
                    X = newX;
                    Value = p.Value;
                }
        }
    }
}
