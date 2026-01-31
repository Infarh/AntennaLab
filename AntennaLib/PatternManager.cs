using MathCore;

namespace Antennas;

/// <summary>Менеджер для расчёта и управления диаграммой направленности антенны</summary>
public class PatternManager
{
    private readonly Antenna _Antenna;
    private readonly double _f0;
    private readonly double _Th1;
    private readonly double _Th2;
    private readonly double _dth0;
    private readonly double _Phi;

    private readonly ObservableLinkedList<PatternValue> _Values = new();

    /// <summary>Начальный угол места для расчёта диаграммы</summary>
    public double Th1 => _Th1;
    
    /// <summary>Конечный угол места для расчёта диаграммы</summary>
    public double Th2 => _Th2;
    
    /// <summary>Фиксированный азимутальный угол</summary>
    public double Phi => _Phi;

    /// <summary>Частота</summary>
    public double F0 => _f0;

    /// <summary>Коллекция значений диаграммы направленности</summary>
    public ObservableLinkedList<PatternValue> Values => _Values; 

    /// <summary>Инициализация менеджера диаграммы направленности</summary>
    /// <param name="antenna">Объект антенны</param>
    /// <param name="f0">Частота расчёта диаграммы</param>
    /// <param name="phi">Азимутальный угол (по умолчанию 0)</param>
    /// <param name="th1">Начальный угол места (по умолчанию -π)</param>
    /// <param name="th2">Конечный угол места (по умолчанию π)</param>
    /// <param name="dth0">Шаг по углу места (по умолчанию 1°)</param>
    /// <param name="eps_db">Точность в дБ, должна быть отрицательной (по умолчанию -30)</param>
    /// <exception cref="ArgumentOutOfRangeException">Если eps_db больше 0 или f0 меньше или равна 0</exception>
    /// <exception cref="ArgumentNullException">Если antenna равна null</exception>
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