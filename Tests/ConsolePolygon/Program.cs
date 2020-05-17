using System;

namespace ConsolePolygon
{
    public static class Program
    {
        private static double find(this Func<double, double> f, double Xmin, double Xmax, double eps)
        {
            while(Math.Abs(Xmax - Xmin) > eps)
            {
                Xmin = Xmax - (Xmax - Xmin) * f(Xmax) / (f(Xmax) - f(Xmin));
                Xmax = Xmin - (Xmin - Xmax) * f(Xmin) / (f(Xmin) - f(Xmax));
            }

            return Xmax;
        }

        private static double IntegralValue(this Func<double, double> f, double a, double b, double dx = 0.001)
        {
            var I = 0d;
            while(a < b) I += f(a += dx) + f(a);
            return I * dx / 2;
        }

        [STAThread]
        private static void Main(string[] args)
        {
            //AdaptiveSamplingTest.Main();
            //ObjectModel.Test();
            BeamPatternExpression.Test();
            BeamPatternAnalisysTest.Test();
            Console.ReadLine();

            //FDTD.Main();
        }

        //private static Complex BeamPattern(double[] R, double Theta)
        //{
        //    var N = R.Length;
        //    var result = new Complex();

        //    for(var i = 0; i < N; i++)
        //        result += Complex.Exp(Phi(R[i], Theta));

        //    var r = BeamPattern0(Theta) * result / N;
        //    return r;
        //}

        //private static Complex BeamPattern0(double Theta) { return 1; }

        //private static double Phi(double x, double Theta) { return k0 * x * Math.Sin(Theta); }


        //private static void Test()
        //{
        //    lambda0 = 30 * 0.01; // Длина волны
        //    Console.WriteLine("lambda0 = {0}cm", lambda0 / 0.01);
        //    f0 = Consts.SpeedOfLight / lambda0;
        //    Console.WriteLine("f0 = {0}ГГц", f0 / 1e9);

        //    k0 = Consts.pi2 / lambda0;
        //    Console.WriteLine("Волновое число k0={0:f3}рад/см = {1:f3}град/см", k0 / 100, k0 * Consts.Geometry.ToDeg / 100);

        //    N = 8;
        //    Console.WriteLine("Число элементов решётки N={0}", N);
        //    d0 = lambda0;
        //    Console.WriteLine("Шаг элементов в решётке d0={0}см", d0 * 100);
        //    L = d0 * (N - 1);
        //    Console.WriteLine("Длина решётки L={0}см", L * 100);

        //    Theta07 = 51 * lambda0 / L / 2;
        //    Console.WriteLine("Ширина луча 2*Theta07={0:f3}град, Theta07={1:f3}", 2 * Theta07, Theta07);


        //    Console.WriteLine("Рассчёт ДН");

        //    var elements = new Antenna[N];
        //    for(var i = 0; i < N; i++)
        //        elements[i] = new Vibrator(lambda0 / 2);

        //    var array = AntennaArray.CreateLinearArray(elements, d0);

        //    for(var i = 0; i < array.Count; i++)
        //        Console.WriteLine("R[{1}]:{2}{0}", (array[i].Location.X * 100), i, array[i].Location.X < 0 ? "" : " ");


        //    var pattern = array.GetPatternOfThetaOnFreq(f0);
        //    Console.WriteLine("Theta\tf\t|f|\targ(f)\t|f|db");

        //    for(var thetta = -30.0; thetta <= 30; thetta += 0.5)
        //    {
        //        var f = pattern(thetta.ToRad());

        //        Console.Write(thetta);
        //        Console.Write("\t");
        //        Console.Write(f.Round(2));
        //        Console.Write("\t");
        //        Console.Write(f.Abs.Round(2));
        //        Console.Write("\t");
        //        Console.Write(f.Arg.Round(2));
        //        Console.Write("\t");
        //        Console.WriteLine(f.Abs.In_dB().Round(2));
        //    }

        //}
    }
}
