using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;

using Antennas;
using Antennas.Calculation;

using ArrayFactor.Service.AmplitudeDistributions;

using MathCore;
using MathCore.WPF.ViewModels;

using OxyPlot;

using static System.Math;
using static MathCore.Consts;

using Distribution = ArrayFactor.Service.AmplitudeDistributions.Distribution;

namespace ArrayFactor;

/// <summary>View-модель главного окна программы</summary>
[MarkupExtensionReturnType(typeof(MainWindowModel))]
internal class MainWindowModel : ViewModel
{
    /* ------------------------------------------------------------------------------------------------ */

    #region Константы

    /// <summary>Константа мультипликативного преобразования углового значения из градусов в радианы</summary>
    private const double c_ToRad = Geometry.ToRad;

    /// <summary>Гигагерцы x10^9</summary>
    private const double c_GHz = 1e9;

    #endregion

    /* ------------------------------------------------------------------------------------------------ */

    #region Поля

    /// <summary>Частота работы модели</summary>
    private double _f0 = 1;

    /// <summary>Длина волны</summary>
    private double _λ0 = 30;

    /// <summary>Угол места отклонения луча решётки</summary>
    private double _θ0;

    /// <summary>Азимут отклонения луча решётки</summary>
    private double _φ0;

    /// <summary>Антенная решётка</summary>
    private readonly RectangularAntennaArray _AntennaArray = new(16, 1, 0.15, 0.15, new UniformAntenna(), (_, _) => 1);

    /// <summary>Угол сечения пространственной ДН решётки</summary>
    private double _PhiView;

    /// <summary>Параллельный запрос, выдающий значения аргумента для расчёта значений амплитудного распределения</summary>
    private readonly ParallelQuery<double> _NormX;

    /// <summary>Объект амплитудного распреления</summary>
    private Distribution? _Distribution;

    /// <summary>Функция амплитудного распределения</summary>
    private Func<double, double, double> _A = null!;

    /// <summary>Шаг расчёта ДН решётки</summary>
    private double _dθ = 1;

    /// <summary>Шаг расчёта амплитудного распределения</summary>
    private double _dx = 0.01;

    /// <summary>Максимум ДН антенной решётки в дБ (20·log10)</summary>
    private double _MaxFieldDb;

    /// <summary>Угловое положение максимума ДН</summary>
    private double _MaxPos = double.NaN;

    /// <summary>КНД сечения ДН антенной решётки</summary>
    private double _D0 = double.NaN;

    /// <summary>УБЛ сечения ДН антенной решётки</summary>
    private double _SSL = double.NaN;

    /// <summary>Нормированное значение УБЛ</summary>
    private double _SSL_Value = double.NaN;

    /// <summary>Средний УБЛ</summary>
    private double _MeanSSL = double.NaN;

    /// <summary>Нормированное значение среднего УБЛ</summary>
    private double _MeanSSL_Value = double.NaN;

    /// <summary>ШИрина луча по уровню -3дБ</summary>
    private double _BeamWidth07 = double.NaN;

    /// <summary>Ширина луча по уровню 0</summary>
    private double _BeamWidth0 = double.NaN;

    /// <summary>Расчётное положение максимума ДН</summary>
    private double _BeamPos = double.NaN;

    /// <summary>Угловое положение левой границы луча</summary>
    private double _BeamLeftAdge07 = double.NaN;

    /// <summary>Угловое положение правой границы луча</summary>
    private double _BeamRightAdge07 = double.NaN;

    /// <summary>Массив значений ДН антенной решётки</summary>
    private PatternValue[] _BeamData = null!;

    /// <summary>Массив значений ДН элемента антенной решётки</summary>
    private PatternValue[] _Beam0Data = null!;

    /// <summary>Источник признаков отмены асинхронной операции расчёта ДН антенной решётки</summary>
    private CancellationTokenSource? _ComputePatternCancellationTokenSource;

    /// <summary>Номер актуального запроса расчёта ДН</summary>
    private int _ComputePatternVersion;

    /// <summary>Массив значений тепловой карты трёхмерной ДН</summary>
    private double[,]? _PatternData;

    /// <summary>Значение прогресса вычисления полного КНД трёхмерной ДН антенной решётки</summary>
    private double? _PatternComplete;

    #endregion

    /* ------------------------------------------------------------------------------------------------ */

    #region Свойства

    /// <summary>Антенная решётка</summary>
    [ChangedHandler(nameof(ComputePatternAsync))]
    public RectangularAntennaArray AntennaArray => _AntennaArray;

    /// <summary>Массив данных с картой ДН</summary>
    public double[,]? PatternData { get => _PatternData; set => Set(ref _PatternData, value); }

    /// <summary>Прогресс вычисления полного КНД</summary>
    public double? PatternComplete { get => _PatternComplete; private set => Set(ref _PatternComplete, value, v => v is null or >= 0 and <= 1); }

    /// <summary>Шаг вычисления ДН</summary>
    [ChangedHandler(nameof(ComputePatternAsync))]
    public double dθ { get => _dθ; set => Set(ref _dθ, value, θ => θ is >= 0.1 and < 5); }

    /// <summary>Выбранное амплитудное распределение</summary>
    [ChangedHandler(nameof(SetDistribution))]
    public Distribution? Distribution
    {
        get => _Distribution;
        set
        {
            if (Equals(_Distribution, value) || value is null) return;

            if (_Distribution != null) _Distribution.PropertyChanged -= OnDistributionPropertyChanged;
            _Distribution = value;
            if (_Distribution != null) _Distribution.PropertyChanged += OnDistributionPropertyChanged;
            _A = (x, y) => _Distribution!.Value(x, y, 0);
            OnPropertyChanged();
        }
    }

    /// <summary>Расчитанные дначения амплитудного распределения</summary>
    public IEnumerable<DataPoint> DistributionData
    {
        get
        {
            double A(double x) => _A(x * Cos(_PhiView * c_ToRad), x * Sin(_PhiView * c_ToRad));
            return _NormX.Select(x => new DataPoint(x, A(x)));
        }
    }

    /// <summary>Максимум ДН в дБ</summary>
    public double MaxFieldDb { get => _MaxFieldDb; private set => Set(ref _MaxFieldDb, value); }

    /// <summary>Положение максимума ДН</summary>
    public double MaxPos { get => _MaxPos; private set => Set(ref _MaxPos, value, v => double.IsNaN(v) || v is >= -180 and <= 180); }

    /// <summary>КНД</summary>
    public double D0 { get => _D0; private set => Set(ref _D0, value); }

    /// <summary>Уровень боковых лепестков</summary>
    public double SSL { get => _SSL; private set => Set(ref _SSL, value); }

    /// <summary>Нормированный (к усилению) УБЛ</summary>
    public double SSL_Value { get => _SSL_Value; private set => Set(ref _SSL_Value, value); }

    /// <summary>Средний УБЛ</summary>
    public double MeanSSL { get => _MeanSSL; private set => Set(ref _MeanSSL, value); }

    /// <summary>Средний нормированный (к усилению) УБЛ</summary>
    public double MeanSSL_Value { get => _MeanSSL_Value; private set => Set(ref _MeanSSL_Value, value); }

    /// <summary>Ширина луча по уровню 0дБ</summary>
    public double BeamWidth0 { get => _BeamWidth0; private set => Set(ref _BeamWidth0, value); }

    /// <summary>Ширина луча по уровню -3дБ</summary>
    public double BeamWidth07 { get => _BeamWidth07; private set => Set(ref _BeamWidth07, value); }

    /// <summary>Расчитанное положение луча</summary>
    public double BeamPos { get => _BeamPos; private set => Set(ref _BeamPos, value); }

    /// <summary>Левая граница луча по уровню -3дБ</summary>
    public double BeamLeftAdge07 { get => _BeamLeftAdge07; private set => Set(ref _BeamLeftAdge07, value); }

    /// <summary>Правая граница луча по уровню -3дБ</summary>
    public double BeamRightAdge07 { get => _BeamRightAdge07; private set => Set(ref _BeamRightAdge07, value); }

    /// <summary>Массив значений ДН решётки</summary>
    public PatternValue[] BeamData { get => _BeamData; private set => Set(ref _BeamData!, value, b => b != null); }

    /// <summary>Массив значений ДН антенного элемента</summary>
    public PatternValue[] Beam0Data { get => _Beam0Data; private set => Set(ref _Beam0Data!, value); }

    /// <summary>Азимутальный угол расчёта ДН</summary>
    public double PhiView
    {
        get => _PhiView;
        set
        {
            if (!Set(ref _PhiView, value, v => v is >= 0 and <= 90)) return;
            OnPropertyChanged(nameof(DistributionData));
            ComputePatternAsync();
        }
    }

    /// <summary>Установленное значение угла отклонения луча решётки</summary>
    [ChangedHandler(nameof(SetDistribution))]
    public double θ0 { get => _θ0; set => Set(ref _θ0, value); }

    /// <summary>Установленное значение азимута отклонения луча решётки</summary>
    [ChangedHandler(nameof(SetDistribution))]
    public double φ0 { get => _φ0; set => Set(ref _φ0, value); }

    /// <summary>Частота</summary>
    [ChangedHandler(nameof(SetDistribution))]
    public double f0
    {
        get => _f0;
        set
        {
            if (Set(ref _f0, value, f => f > 0)) Set(ref _λ0, 30 / value, l => l > 0, PropertyName: nameof(λ0));
        }
    }

    /// <summary>Длина волны</summary>
    public double λ0
    {
        get => _λ0;
        set
        {
            if (Set(ref _λ0, value, l => l > 0)) Set(ref _f0, 30 / value, f => f > 0, PropertyName: nameof(f0));
        }
    }

    /// <summary></summary>
    [ChangedHandler(nameof(SetDistribution))]
    public int Nx
    {
        get => _AntennaArray.Nx;
        set
        {
            if (_AntennaArray.Nx.Equals(value)) return;

            _AntennaArray.Nx = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Число элементов решётки вдоль оси OY</summary>
    [ChangedHandler(nameof(SetDistribution))]
    public int Ny
    {
        get => _AntennaArray.Ny;
        set
        {
            if (_AntennaArray.Ny.Equals(value)) return;

            _AntennaArray.Ny = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Шаг между элементами решётки вдоль оси OX</summary>
    [ChangedHandler(nameof(SetDistribution))]
    public double dx
    {
        get => _AntennaArray.dx * 100;
        set
        {
            value /= 100;
            if (_AntennaArray.dx.Equals(value)) return;

            _AntennaArray.dx = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Шаг между элементами решётки вдоль оси OY</summary>
    [ChangedHandler(nameof(SetDistribution))]
    public double dy
    {
        get => _AntennaArray.dy * 100;
        set
        {
            value /= 100;
            if (_AntennaArray.dy.Equals(value)) return;

            _AntennaArray.dy = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Размер апертуры вдоль оси OX</summary>
    [DependsOn(nameof(Nx)), DependsOn(nameof(dx))]
    public double Lx => _AntennaArray.L_x;

    /// <summary>Размер апертуры вдоль оси OY</summary>
    [DependsOn(nameof(Ny)), DependsOn(nameof(dy))]
    public double Ly => _AntennaArray.L_y;

    /// <summary>Ширина луча вдоль оси OX</summary>
    [DependsOn(nameof(f0)), DependsOn(nameof(Lx))]
    public double θ2_07_x => 51 * _λ0 / _AntennaArray.L_x / 100;

    /// <summary>Ширина луча вдоль оси OY</summary>
    [DependsOn(nameof(f0)), DependsOn(nameof(Ly))]
    public double θ2_07_y => 51 * _λ0 / _AntennaArray.L_y / 100;

    /// <summary>Сектор сканирования вдоль оси OX</summary>
    [DependsOn(nameof(f0)), DependsOn(nameof(dx))]
    public double θmax_x => GetScanSector(_λ0, dx);

    /// <summary>Сектор сканирования вдоль оси OY</summary>
    [DependsOn(nameof(f0)), DependsOn(nameof(dy))]
    public double θmax_y => GetScanSector(_λ0, dy);

    /// <summary>Вычислить предельный угол сканирования до появления дифракционного максимума</summary>
    /// <param name="WaveLength">Длина волны в сантиметрах</param>
    /// <param name="Step">Шаг между элементами в сантиметрах</param>
    /// <returns>Предельный угол сканирования в градусах</returns>
    private static double GetScanSector(double WaveLength, double Step)
    {
        var q = WaveLength / Step;
        return q switch
        {
            >= 2 => 90,
            >= 1 => Asin(q - 1).ToDeg(),
            _ => 0
        };
    }

    /// <summary>Элементарный излучатель</summary>
    [AffectsThe(nameof(AntennaArray))]
    public Antenna ElementaryAntenna
    {
        get => _AntennaArray.Element;
        set
        {
            if (ReferenceEquals(_AntennaArray.Element, value)) return;

            var old_value = _AntennaArray.Element;
            {
                if (old_value is INotifyPropertyChanged changed_notify_obj)
                    changed_notify_obj.PropertyChanged -= OnElementaryAntennaChanged;
            }
            _AntennaArray.Element = value;
            {
                if (value is INotifyPropertyChanged changed_notify_obj)
                    changed_notify_obj.PropertyChanged += OnElementaryAntennaChanged;
            }
            OnPropertyChanged();
        }
    }

    #endregion

    #region Команды

    #endregion

    /* ------------------------------------------------------------------------------------------------ */

    #region Конструктор

    /// <summary>Инициализация нового экземпляра <see cref="MainWindowModel"/></summary>
    public MainWindowModel()
    {
        _NormX = GetXRange().AsParallel().AsOrdered();
        Distribution = new Uniform();
        Nx++;
    }

    #endregion

    /* ------------------------------------------------------------------------------------------------ */

    #region Методы

    /// <summary>Обработчик событий внутри объекта амплитудного распределения, вызывающий перерасчёт значений амплитудного распределения</summary>
    /// <param name="sender">Источник события - экземпляр амплитудного распределения</param>
    /// <param name="args">Аргумент события, несущий имя изменившегося свойства</param>
    private void OnDistributionPropertyChanged(object? sender, PropertyChangedEventArgs args) => SetDistribution();

    /// <summary>Получить пречисление значений аргумента для расчёта функции амплитудного распределения</summary>
    /// <returns></returns>
    private IEnumerable<double> GetXRange()
    {
        var dx = _dx;
        for (var x = -0.5; x < 0.5; x += dx)
            yield return x;

        yield return 0.5;
    }

    /// <summary>Запустить асинхронный расчёт диаграммы направленности</summary>
    private void ComputePatternAsync() => _ = ComputePatternCoreAsync();

    /// <summary>Асинхронно рассчитать диаграмму направленности по неизменяемому снимку входных данных</summary>
    private async Task ComputePatternCoreAsync()
    {
        var request_id = ++_ComputePatternVersion;

        var old_source = _ComputePatternCancellationTokenSource;
        old_source?.Cancel();
        old_source?.Dispose();

        var source = new CancellationTokenSource();
        _ComputePatternCancellationTokenSource = source;
        var cancel = source.Token;

        try
        {
            // снимок формируется в UI-потоке, фоновый расчёт не читает поля модели
            var array = new RectangularAntennaArray(
                _AntennaArray.Nx, _AntennaArray.Ny, _AntennaArray.dx, _AntennaArray.dy,
                _AntennaArray.Element, _AntennaArray.Distribution);

            var request = new PatternRequest(
                array, _AntennaArray.Element, _f0 * c_GHz, _PhiView * c_ToRad,
                -180 * c_ToRad, 180 * c_ToRad, _dθ * ToRad);

            var progress = new Progress<double>(value =>
            {
                if (request_id == _ComputePatternVersion) PatternComplete = value;
            });

            var result = await Task.Run(
                () => PatternCalculator.Calculate(request, progress, cancel),
                cancel).ConfigureAwait(true);

            if (cancel.IsCancellationRequested || request_id != _ComputePatternVersion) return;

            BeamData = result.ArrayPattern;
            Beam0Data = result.ElementPattern;
            PatternData = result.HeatMap;
            ApplyAnalysis(result.Analysis);
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        finally
        {
            if (ReferenceEquals(_ComputePatternCancellationTokenSource, source))
            {
                _ComputePatternCancellationTokenSource = null;
                source.Dispose();
            }
        }
    }

    /// <summary>Применить результаты анализа ДН к свойствам модели</summary>
    /// <param name="result">Результат анализа ДН</param>
    private void ApplyAnalysis(AnalysePatternResult result)
    {
        MaxPos = result.MaxAngle.ToDeg();
        MaxFieldDb = result.Max.In_dB();
        D0 = result.D0.In_dB_byPower();

        var ssl = result.SSL.In_dB();
        SSL = ssl;
        SSL_Value = ssl - MaxFieldDb;

        var mean_ssl = result.MeanSLL.In_dB();
        MeanSSL = mean_ssl;
        MeanSSL_Value = mean_ssl - MaxFieldDb;

        BeamPos = result.MaxAngle.ToDeg();
        BeamWidth07 = (result.Th07Right - result.Th07Left).ToDeg();
        BeamWidth0 = (result.Th0Right - result.Th0Left).ToDeg();
        BeamLeftAdge07 = result.Th07Left.ToDeg();
        BeamRightAdge07 = result.Th07Right.ToDeg();
    }

    /// <summary>Обработчик событий изменений параметров антенного элемента</summary>
    /// <param name="sender">Источник события - антенный элемент</param>
    /// <param name="args">Аргумент события, несущий имя изменившегося свойства</param>
    private void OnElementaryAntennaChanged(object? sender, PropertyChangedEventArgs args) => OnPropertyChanged(nameof(ElementaryAntenna));

    /// <summary>Метод установки амплитудного распределения</summary>
    private void SetDistribution()
    {
        var phase = GetPhaseDistribution(_θ0 * c_ToRad, _φ0 * c_ToRad, _f0 * c_GHz);
        var lx = Lx;
        var ly = Ly;
        var infx = lx == 0;
        var infy = ly == 0;
        var ilx = 1 / lx;
        var ily = 1 / ly;
        switch (infx)
        {
            case false when !infy:
                _AntennaArray.Distribution = (x, y) => Complex.Exp(_A(x * ilx, y * ily), phase(x, y));
                break;
            case true when !infy:
                _AntennaArray.Distribution = (_, y) => Complex.Exp(_A(0, y * ily), phase(0, y));
                break;
            case false when infy:
                _AntennaArray.Distribution = (x, _) => Complex.Exp(_A(x * ilx, 0), phase(x, 0));
                break;
            default:
                {
                    var A = _A(0, 0);
                    _AntennaArray.Distribution = (_, _) => A;
                    break;
                }
        }

        OnPropertyChanged(nameof(DistributionData));
        ComputePatternAsync();
    }

    /// <summary>Получение функции фазового распределения апертуры для заданной частоты</summary>
    /// <param name="Theta">Угол места фазирования антенной решётки</param>
    /// <param name="Phi">Угол азимута фазирования антенной решётки</param>
    /// <param name="f">Частота расчёта фазовго распределения</param>
    /// <returns>Функция фазовго распределения</returns>
    private static Func<double, double, double> GetPhaseDistribution(double Theta, double Phi, double f)
    {
        var sinTh = Sin(Theta) * (pi2 / SpeedOfLight * f);
        var cosPh = Cos(Phi) * sinTh;
        var sinPh = Sin(Phi) * sinTh;
        return (x, y) => x * cosPh + y * sinPh;
    }

    #endregion

    /* ------------------------------------------------------------------------------------------------ */
}
