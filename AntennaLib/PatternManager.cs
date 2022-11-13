using MathCore;

namespace Antennas;

public class PatternManager
{
    private readonly Antenna _Antenna;
    private readonly double _f0;
    private readonly double _Th1;
    private readonly double _Th2;
    private readonly double _dth0;
    private readonly double _Phi;

    private readonly ObservableLinkedList<PatternValue> _Values = new();

    public double Th1 => _Th1;
    public double Th2 => _Th2;
    public double Phi => _Phi;

    public double F0 => _f0;

    public ObservableLinkedList<PatternValue> Values => _Values; 

    public PatternManager(Antenna antenna, double f0,
        double phi = 0, double th1 = Consts.pi_neg, double th2 = Consts.pi, double dth0 = 1 * Consts.ToRad,
        double eps_db = -30)
    {
        if (eps_db > 0) throw new ArgumentOutOfRangeException(nameof(eps_db), @"Значение точности в дБ должно быть меньше 0");
        if(f0 <= 0) throw new ArgumentOutOfRangeException(nameof(f0), @"Частота должна быть больше 0");

        _Antenna = antenna ?? throw new ArgumentNullException(nameof(antenna));
        _f0      = f0;
        _Th1     = th1;
        _Th2     = th2;
        _dth0    = dth0;
        _Phi     = phi;
    }
}