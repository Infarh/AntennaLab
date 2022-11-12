#nullable enable
using MathCore;
using MathCore.Vectors;

namespace Antennas;

public delegate Complex Distribution(double x, double y);

/// <summary>Прямоугольная плоская антенная решётка</summary>
public class RectangularAntennaArray : AntennaArray
{
    /// <summary>Метод инициализации элементов решётки</summary>
    /// <param name="Nx">Число элементов по оси X</param>
    /// <param name="Ny">Число элементов по оси Y</param>
    /// <param name="dx">Шаг между элементами по оси X</param>
    /// <param name="dy">Шаг между элементами по оси Y</param>
    /// <param name="Element">Антенный элемент</param>
    /// <param name="Distribution">Распределение</param>
    /// <returns>Перечисление антенных элементов решётки</returns>
    private static IEnumerable<AntennaItem> Initialize(int Nx, int Ny, double dx, double dy, Antenna Element, Distribution Distribution)
    {
        var Lx = (Nx - 1) * dx;
        var Ly = (Ny - 1) * dy;
        var x0 = Lx / 2;
        var y0 = Ly / 2;

        var angle = new SpaceAngle();
        for(var ix = 0; ix < Nx; ix++)
            for(var iy = 0; iy < Ny; iy++)
            {
                var x = ix * dx - x0;
                var y = iy * dy - y0;
                var k = Distribution(x, y);
                yield return new AntennaItem(Element, new(x, y), angle, k);
            }
    }

    /// <summary>Число элементов по оси X</summary>
    private int _Nx;
    /// <summary>Число элементов по оси Y</summary>
    private int _Ny;
    /// <summary>Шаг между элементами по оси X</summary>
    private double _dx;
    /// <summary>Шаг между элементами по оси Y</summary>
    private double _dy;
    /// <summary>Антенный элемент</summary>
    private Antenna _Element;
    /// <summary>Распределение</summary>
    private Distribution _Distribution;


    /// <summary>Число элементов по оси X</summary>
    public int Nx
    {
        get => _Nx;
        set
        {
            if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
            if(_Nx == value) return;
            _Nx = value;
            RefreshConstruction();
        }
    }

    /// <summary>Число элементов по оси Y</summary>
    public int Ny
    {
        get => _Ny;
        set
        {
            if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
            if(_Ny == value) return;
            _Ny = value;
            RefreshConstruction();
        }
    }

    /// <summary>Шаг между элементами по оси X</summary>
    public double dx
    {
        get => _dx;
        set
        {
            if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
            if(_dx.Equals(value)) return;
            _dx = value;
            RefreshGeometry();
        }
    }

    /// <summary>Шаг между элементами по оси Y</summary>
    public double dy
    {
        get => _dy;
        set
        {
            if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
            if(_dy.Equals(value)) return;
            _dy = value;
            RefreshGeometry();
        }
    }

    /// <summary>Антенный элемент</summary>
    public Antenna Element
    {
        get => _Element;
        set
        {
            if(value is null) throw new ArgumentNullException(nameof(value));
            if(ReferenceEquals(_Element, value)) return;
            _Element = value;
            for(var i = 0; i < Count; i++) this[i].Element = value;
        }
    }

    /// <summary>Распределение</summary>
    public Distribution Distribution
    {
        get => _Distribution;
        set
        {
            if(value is null) throw new ArgumentNullException(nameof(value));
            if(ReferenceEquals(_Distribution, value)) return;
            _Distribution = value;
            for(var i = 0; i < Count; i++)
            {
                var a = this[i];
                var (x, y, _) = a.Location;
                a.K           = _Distribution(x, y);
            }
        }
    }

    /// <summary>Инициализация прямоугольной плоской антенной решётки</summary>
    /// <param name="Nx"></param>
    /// <param name="Ny"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <param name="Element"></param>
    /// <param name="Distribution"></param>
    public RectangularAntennaArray(int Nx, int Ny, double dx, double dy, Antenna Element, Distribution Distribution)
        : base(Initialize(Nx, Ny, dx, dy, Element, Distribution))
    {
        _Nx           = Nx;
        _Ny           = Ny;
        _dx           = dx;
        _dy           = dy;
        _Element      = Element;
        _Distribution = Distribution;
    }

    private void RefreshConstruction()
    {
        Clear();
        AddRange(Initialize(_Nx, _Ny, _dx, _dy, _Element, _Distribution));
    }

    private void RefreshGeometry()
    {
        var Nx = _Nx;
        var Ny = _Ny;
        var dx = _dx;
        var dy = _dy;
        var A  = _Distribution;

        var Lx = (Nx - 1) * dx;
        var Ly = (Ny - 1) * dy;
        var x0 = Lx / 2;
        var y0 = Ly / 2;

        for(var ix = 0; ix < Nx; ix++)
            for(var iy = 0; iy < Ny; iy++)
            {
                var i = Ny * ix + iy;
                var a = this[i];
                var x = ix * dx - x0;
                var y = iy * dy - y0;
                a.Location = new(x, y);
                a.K        = A(x, y);
            }
    }
}