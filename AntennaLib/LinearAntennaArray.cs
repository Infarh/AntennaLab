namespace Antennas;

/// <summary>Линейная антенная решётка с элементами, расположенными на одной прямой</summary>
public class LinearAntennaArray : AntennaArray
{
    /// <summary>Инициализация линейной антенной решётки</summary>
    /// <param name="elements">Перечисление антенных элементов</param>
    /// <param name="d">Шаг между элементами</param>
    public LinearAntennaArray(IEnumerable<Antenna> elements, double d)
        : base(elements.Select((a, i) => new AntennaItem(a, new(i * d), new(), 1)))
    {
        var L2 = L_x / 2;
        for(var i = 0; i < Count; i++)
            this[i].Location = this[i].Location.DecX(L2);
    }

    /// <summary>Инициализация линейной антенной решётки с унифицированными элементами</summary>
    /// <param name="N">Число элементов в решётке</param>
    /// <param name="d">Шаг между элементами</param>
    public LinearAntennaArray(int N, double d) : this(new Antenna[N].Initialize(_ => new UniformAntenna()), d) { }
}