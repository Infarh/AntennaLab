using System.Text;
using Antennas;
using MathCore;

namespace ConsolePolygon;

internal class Model
{
    public double Lambda0 { get; set; }
    public double f0 { get => Consts.SpeedOfLight / Lambda0; set => Lambda0 = Consts.SpeedOfLight / value; }
    public double k0 => Consts.pi2 / Lambda0;
    public int N { get; set; }

    public double ThetaMax { get; set; }
    public double d0 => Lambda0 / (1 + Math.Abs(Math.Sin(ThetaMax)));
    public double L => d0 * (N - 1);
    public double Theta07_deg => 51 * Lambda0 / L / 2;
    public double Theta07_rad => 0.89012 * Lambda0 / L / 2;

    public Model(double Lambda = 30 * 0.01, int N = 32, double Theta0Max = 0)
    {
        Lambda0  = Lambda;
        this.N   = N;
        ThetaMax = Theta0Max;
    }

    public AntennaArray GetArray() => new LinearAntennaArray(N, d0);

    public Func<double, Complex> GetPattern() => GetArray().GetPatternOfPhiOnFreq(f0);

    public override string ToString() =>
        new StringBuilder("Модель:\r\n")
           .AppendFormat("lambda0 = {0}cm\r\n", Lambda0 / 0.01)
           .AppendFormat("f0 = {0}ГГц\r\n", f0 / 1e9)
           .AppendFormat("Волновое число k0={0:f3}рад/см = {1:f3}град/см\r\n", k0 / 100, k0 * Consts.Geometry.ToDeg / 100)
           .AppendFormat("Число элементов решётки N={0}\r\n", N)
           .AppendFormat("Шаг элементов в решётке d0={0}см\r\n", d0 * 100)
           .AppendFormat("Длина решётки L={0}см\r\n", L * 100)
           .AppendFormat("Ширина луча 2*Theta07={0:f3}град, Theta07={1:f3}\r\n", 2 * Theta07_deg, Theta07_deg)
           .ToString();
}