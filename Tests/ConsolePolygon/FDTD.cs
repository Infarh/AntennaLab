using System;
using System.Threading.Tasks;

namespace ConsolePolygon
{
    internal static class FDTD
    {
        public static void Run() => Test1();

        private static void Test1()
        {
            const int SIZE = 200;

            var ez = new double[SIZE];
            var hy = new double[SIZE];

            const double imp0 = 377;

            const int maxTime = 1000;

            // Циклы времени
            for(var qTime = 0; qTime < maxTime; qTime++)
            {
                UpdateH(ez, hy, imp0);
                UpdateE(ez, hy, imp0);

                ez[0] = fE1(qTime, 30, 100);

                Console.WriteLine($"{qTime:d3}|ez[50]:{ez[50]:f3}");
            }

            Console.ReadLine();
        }

        private static void UpdateE(double[] e, double[] h, double z)
        {
            for(var mm = 1; mm < e.Length; mm++)
                e[mm] += (h[mm] - h[mm - 1]) * z;
        }

        private static void UpdateH(double[] e, double[] h, double z)
        {
            for(var mm = 0; mm < h.Length - 1; mm++)
                h[mm] += (e[mm + 1] - e[mm]) / z;
        }

        private static async Task UpdateE
        (
            double[,,] Hx, double[,,] Hy, double[,,] Hz,
            double[,,] Ex, double[,,] Ey, double[,,] Ez,
            double tx, double ty, double tz,
            double[,,] AE
        )
        {
            var Lx = Ex.GetLength(0);
            var Ly = Ex.GetLength(1);
            var Lz = Ex.GetLength(2);

            var tEx = Task.Factory.StartNew(() =>
            {
                for(var i = 0; i < Lx - 1; i++)
                    for(var j = 1; j < Ly - 1; j++)
                        for(var k = 1; k < Lz - 1; k++)
                            Ex[i, j, k] = Ex[i, j, k] * AE[i, j, k]
                                + ((Hz[i, j, k] - Hz[i, j - 1, k]) / ty - (Hy[i, j, k] - Hy[i, j, k - 1]) / tz);
            });

            var tEy = Task.Factory.StartNew(() =>
            {
                for(var i = 1; i < Lx - 1; i++)
                    for(var j = 0; j < Ly - 1; j++)
                        for(var k = 1; k < Lz - 1; k++)
                            Ey[i, j, k] = Ey[i, j, k] * AE[i, j, k]
                                + ((Hx[i, j, k] - Hx[i, j, k - 1]) / tz - (Hz[i, j, k] - Hz[i - 1, j, k]) / tx);
            });

            var tEz = Task.Factory.StartNew(() =>
            {
                for(var i = 1; i < Lx - 1; i++)
                    for(var j = 1; j < Ly - 1; j++)
                        for(var k = 0; k < Lz - 1; k++)
                            Ez[i, j, k] = Ez[i, j, k] * AE[i, j, k]
                                + ((Hy[i, j, k] - Hy[i - 1, j, k]) / tx - (Hx[i, j, k] - Hx[i, j - 1, k]) / ty);
            });

            await Task.WhenAll(tEx, tEy, tEz).ConfigureAwait(false);
        }

        private static async Task UpdateH
        (
            double[,,] Hx, double[,,] Hy, double[,,] Hz,
            double[,,] Ex, double[,,] Ey, double[,,] Ez,
            double tx, double ty, double tz,
            double[,,] AH
        )
        {
            var Lx = Hx.GetLength(0);
            var Ly = Hx.GetLength(1);
            var Lz = Hx.GetLength(2);

            var tHx = Task.Factory.StartNew(() =>
            {
                for(var i = 0; i < Lx; i++)
                    for(var j = 0; j < Ly - 1; j++)
                        for(var k = 0; k < Lz - 1; k++)
                            Hx[i, j, k] -= ((Ez[i, j + 1, k] - Ez[i, j, k]) / ty - (Ey[i, j, k + 1] - Ey[i, j, k]) / tz)
                                            * AH[i, j, k];
            });

            var tHy = Task.Factory.StartNew(() =>
            {
                for(var i = 0; i < Lx - 1; i++)
                    for(var j = 0; j < Ly; j++)
                        for(var k = 0; k < Lz - 1; k++)
                            Hy[i, j, k] -= ((Ex[i, j, k + 1] - Ex[i, j, k]) / tz - (Ez[i + 1, j, k] - Ez[i, j, k]) / tx)
                                                * AH[i, j, k];
            });

            var tHz = Task.Factory.StartNew(() =>
            {
                for(var i = 0; i < Lx - 1; i++)
                    for(var j = 0; j < Ly - 1; j++)
                        for(var k = 0; k < Lz; k++)
                            Hz[i, j, k] -= ((Ey[i + 1, j, k] - Ey[i, j, k]) / tx - (Ex[i, j + 1, k] - Ex[i, j, k]) / ty)
                                                 * AH[i, j, k];
            });

            await Task.WhenAll(tHx, tHy, tHz).ConfigureAwait(false);
        }

        private static double fE1(double t, double t0, double tau)
        {
            t -= t0;
            t *= t;
            t /= tau;
            return Math.Exp(-t);
        }
    }
}