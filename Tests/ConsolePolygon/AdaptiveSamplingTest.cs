using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MathCore;
using MathCore.Annotations;
using MathCore.Vectors;

namespace ConsolePolygon
{
    internal static class AdaptiveSamplingTest
    {
        private static double Sinc(double x) => x.Equals(0d) ? 1 : Math.Sin(x) / x;

        public static void Run()
        {
            const double x1 = 0;
            const double x2 = 5;
            const double l = 0.01;

            var f = new Func<double, double>(x => Sinc((x - 1.5) * Consts.pi2));
            var samples = f.SamplingAdaptive_HalfDivision(x1, x2, l);
            //var eps = samples.ClarifySampling(l);
            //eps = samples.ClarifySampling(l);
        }

        private static void Arrange(ref double x_min, ref double x_max)
        {
            if(x_min <= x_max) return;
            var x = x_min;
            x_min = x_max;
            x_max = x;
        }

        public static SamplingResult SamplingAdaptive(this Func<double, double> f, double x1, double x2, double eps) => new(f, x1, x2, eps);

        public class SamplingResult
        {
            private double _Accuracy;
            private readonly LinkedList<Vector2D> _List;
            [NotNull]
            private readonly Func<double, double> _F;

            public double Accuracy => _Accuracy;

            public SamplingResult([NotNull] Func<double, double> f, double x1, double x2, double dx)
            {
                if(dx <= 0) throw new ArgumentOutOfRangeException(nameof(dx), $"Error: {nameof(dx)} <= 0");
                _List = new LinkedList<Vector2D>();
                _F = f ?? throw new ArgumentNullException(nameof(f));
                Arrange(ref x1, ref x2);

                var x = x1;
                var y = f(x);
                var node = _List.AddFirst(new Vector2D(x, y));
                do
                {
                    x += dx;
                    var dy = -y + (y = f(x));
                    var l = Math.Sqrt(dx * dx + dy * dy);
                    l -= dx;
                    _Accuracy += l * l;
                    node = _List.AddAfter(node, new Vector2D(x, y));
                } while(Math.Abs(x2 - x) / dx > 0.25);
                if(!_List.Last.Value.X.Equals(x2))
                    _List.Last.Value = new Vector2D(x2, f(x2));
                _Accuracy = Math.Sqrt(_Accuracy);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public double ClarifySampling(double accuracy)
            {
                if(accuracy <= 0) throw new ArgumentOutOfRangeException(nameof(accuracy), $"Error: {nameof(accuracy)} <= 0");

                lock (_List)
                {
                    var current = _List.First;
                    var result = 0d;
                    var next = current.Next;
                    if(next is null) return 0d;
                    do
                    {
                        var (x0, y0) = current.Value;
                        var (x1, y1) = next.Value;
                        var dx = x1 - x0;
                        var dy = y1 - y0;
                        var l = Math.Sqrt(dx * dx + dy * dy);
                        if(l >= accuracy)
                        {
                            var x11 = x0 + (x1 - x0) * accuracy / l;
                            var y11 = _F(x11);
                            dx = x11 - x0;
                            dy = y11 - y0;
                            l = Math.Sqrt(dx * dx + dy * dy);
                            l = accuracy - l;
                            result += l * l;
                            current = current.AddAfter(new Vector2D(x11, y11));
                        }
                        else if(next.Next != null)
                            _List.Remove(next);
                    } while((current = next).Next != null);

                    return _Accuracy = Math.Sqrt(result);
                }
            }

            public Vector2D[] GetValues() { lock (_List) return _List.OrderBy(v => v.X).ToArray(); }

            public static implicit operator Vector2D[] (SamplingResult result) => result.GetValues();
        }

        public static Vector2D[] SamplingAdaptive_OneWay(this Func<double, double> f, double x1, double x2, double eps, double dx = 0)
        {
            Arrange(ref x1, ref x2);
            var result = new LinkedList<Vector2D>();
            var x = x1;
            if(dx <= 0) dx = eps;
            var y = f(x);
            result.AddLast(new Vector2D(x, y));
            //var accuracy = 0d;
            do
            {
                x += dx;
                var dy = -y + (y = f(x));
                result.AddLast(new Vector2D(x, y));
                var l = Math.Sqrt(dx * dx + dy * dy);
                dx *= eps / l;
                //var dl = l - eps;
                //dl *= dl;
                //accuracy += dl;
            } while(x + dx < x2);

            //Console.WriteLine($"Accuracy:{accuracy}");
            return result.ToArray();
        }

        public static Vector2D[] SamplingAdaptive_HalfDivision(this Func<double, double> f, double x1, double x2, double eps)
        {
            Arrange(ref x1, ref x2);
            var result = new LinkedList<Vector2D>();
            result.AddFirst(new Vector2D(x1, f(x1)));
            result.AddLast(new Vector2D(x2, f(x2)));
            //var accuracy = 0d;
            var node = result.First;
            do
            {
                var v1 = node.Value;
                var v2 = node.Next.Value;

                var l = (v2 - v1).R;
                if(l > eps)
                {
                    var x = (v1.X + v2.X) / 2;
                    result.AddAfter(node, new Vector2D(x, f(x)));
                }
                else
                {
                    var x = (v1.X + v2.X) / 2;
                    var y = f(x);
                    var dx = x - v1.X;
                    var dy = y - v1.Y;
                    l = Math.Sqrt(dx * dx + dy * dy);
                    if(l > eps)
                        result.AddAfter(node, new Vector2D(x, y));
                    else
                    {
                        node = node.Next;
                        //var dl = eps - l;
                        //dl *= dl;
                        //accuracy += dl;
                    }
                }

            } while(node?.Next != null);

            return result.ToArray();
        }
    }
}