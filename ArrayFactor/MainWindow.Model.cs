using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using Antennas;
using ArrayFactor.Service.AmplitudeDistributions;
using MathCore;
using MathCore.WPF.ViewModels;
using OxyPlot;
using Distribution = ArrayFactor.Service.AmplitudeDistributions.Distribution;

namespace ArrayFactor
{
    /// <summary>View-модель главного окна программы</summary>
    [MarkupExtensionReturnType(typeof(MainWindowModel))]
    internal class MainWindowModel : ViewModel
    {
        /* ------------------------------------------------------------------------------------------------ */

        #region Константы

        /// <summary>Значение числа Пи</summary>
        private const double c_Pi = Consts.pi;
        /// <summary>Значение числа Пи/2</summary>
        private const double c_Pi05 = Consts.pi05;
        /// <summary>Константа мультипликативного преобразования углового значения из градусов в радианы</summary>
        private const double c_ToRad = Consts.Geometry.ToRad;
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

        /// <summary>Объект амплитудного распределения</summary>
        private Distribution _Distribution;

        /// <summary>Функция амплитудного распределения</summary>
        private Func<double, double, double> _A;

        /// <summary>Шаг расчёта ДН решётки</summary>
        private double _dθ = 1;

        /// <summary>Шаг расчёта амплитудного распределения</summary>
        private double _dx = 0.01;

        /// <summary>Усиление антенной решётки - максимуМ ДН</summary>
        private double _Gain;

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
        private PatternValue[] _BeamData;

        /// <summary>Массив значений ДН элемента антенной решётки</summary>
        private PatternValue[] _Beam0Data;

        /// <summary>Источник признаков отмены асинхронной операции расчёта ДН антенной решётки</summary>
        private CancellationTokenSource _ComputePatternCancellationTokenSource;

        /// <summary>Информатор прогрессе вычисления полного КНД трёхмерной ДН</summary>
        private readonly IProgress<double?> _PatternProcessProgressReporter;

        /// <summary>Массив значений тепловой карты трёхмерной ДН</summary>
        private double[,] _PatternData;

        /// <summary>Значение прогресса вычисления полного КНД трёхмерной ДН антенной решётки</summary>
        private double? _PatternComplete;

        #endregion

        /* ------------------------------------------------------------------------------------------------ */

        #region Свойства

        /// <summary>Антенная решётка</summary>
        [ChangedHandler(nameof(ComputePatternAsync))]
        public RectangularAntennaArray AntennaArray => _AntennaArray;

        /// <summary>Массив данных с картой ДН</summary>
        public double[,] PatternData { get => _PatternData; set => Set(ref _PatternData, value); }

        /// <summary>Прогресс вычисления полного КНД</summary>
        public double? PatternComplete
        {
            get => _PatternComplete;
            private set => Set(ref _PatternComplete, value, v => v is null || v >= 0 && v <= 1);
        }

        /// <summary>Полный КНД прространственной ДН</summary>
        public double D0_Total { get => Get<double>(); private set => Set(value); }

        /// <summary>Шаг вычисления ДН</summary>
        [ChangedHandler(nameof(ComputePatternAsync))]
        public double dθ { get => _dθ; set => Set(ref _dθ, value, θ => θ >= 0.1 && θ < 5); }

        /// <summary>Выбранное амплитудное распределение</summary>
        [ChangedHandler(nameof(SetDestribution))]
        public Distribution Distribution
        {
            get => _Distribution;
            set
            {
                if (Equals(_Distribution, value) || value is null) return;
                if (_Distribution != null) _Distribution.PropertyChanged -= OnDistributionPropertyChanged;
                _Distribution = value;
                if (_Distribution != null) _Distribution.PropertyChanged += OnDistributionPropertyChanged;
                _A = (x, y) => _Distribution.Value(x, y, 0);
                OnPropertyChanged();
            }
        }

        /// <summary>Рассчитанные значения амплитудного распределения</summary>
        [ChangedHandler(nameof(ComputePatternAsync))]
        public IEnumerable<DataPoint> DistributionData
        {
            get
            {
                double A(double x) => _A(x * Math.Cos(_PhiView * c_ToRad), x * Math.Sin(_PhiView * c_ToRad));
                return _NormX.Select(x => new DataPoint(x, A(x)));
            }
        }

        /// <summary>Усиление антенны</summary>
        public double Gain { get => _Gain; private set => Set(ref _Gain, value); }

        /// <summary>Положение максимума ДН</summary>
        public double MaxPos { get => _MaxPos; private set => Set(ref _MaxPos, value, v => double.IsNaN(v) || v >= -180 && v <= 180); }

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

        /// <summary>Рассчитанное положение луча</summary>
        public double BeamPos { get => _BeamPos; private set => Set(ref _BeamPos, value); }

        /// <summary>Левая граница луча по уровню -3дБ</summary>
        public double BeamLeftAdge07 { get => _BeamLeftAdge07; private set => Set(ref _BeamLeftAdge07, value); }

        /// <summary>Правая граница луча по уровню -3дБ</summary>
        public double BeamRightAdge07 { get => _BeamRightAdge07; private set => Set(ref _BeamRightAdge07, value); }

        /// <summary>Массив значений ДН решётки</summary>
        public PatternValue[] BeamData { get => _BeamData; private set => Set(ref _BeamData, value, b => b != null); }

        /// <summary>Массив значений ДН антенного элемента</summary>
        public PatternValue[] Beam0Data { get => _Beam0Data; private set => Set(ref _Beam0Data, value); }

        /// <summary>Азимутальный угол расчёта ДН</summary>
        [ChangedHandler(nameof(SetDestribution))]
        public double PhiView { get => _PhiView; set => Set(ref _PhiView, value, v => v >= 0 && v <= 90); }

        /// <summary>Установленное значение угла отклонения луча решётки</summary>
        [ChangedHandler(nameof(SetDestribution))]
        public double θ0 { get => _θ0; set => Set(ref _θ0, value); }

        /// <summary>Частота</summary>
        [ChangedHandler(nameof(SetDestribution))]
        public double f0 { get => _f0; set { if (Set(ref _f0, value, f => f > 0)) Set(ref _λ0, 30 / value, l => l > 0, PropertyName: nameof(λ0)); } }

        /// <summary>Длина волны</summary>
        public double λ0 { get => _λ0; set { if (Set(ref _λ0, value, l => l > 0)) Set(ref _f0, 30 / value, f => f > 0, PropertyName: nameof(f0)); } }

        /// <summary></summary>
        [ChangedHandler(nameof(SetDestribution))]
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
        [ChangedHandler(nameof(SetDestribution))]
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
        [ChangedHandler(nameof(SetDestribution))]
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
        [ChangedHandler(nameof(SetDestribution))]
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
        public double θmax_x => Math.Abs(Math.Asin(_λ0 / dx / 100 - 1)).ToDeg();

        /// <summary>Сектор сканирования вдоль оси OY</summary>
        [DependsOn(nameof(f0)), DependsOn(nameof(dy))]
        public double θmax_y => Math.Abs(Math.Asin(_λ0 / dy / 100 - 1)).ToDeg();

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
            D0_Total = double.NaN;
            _PatternProcessProgressReporter = new Progress<double?>(OnD0ProgressReport);
            Nx++;
        }

        #endregion

        /* ------------------------------------------------------------------------------------------------ */

        #region Методы

        private async Task ComputeBeamPatternHeatMapAsync(CancellationToken cancel)
        {
            PatternData = null;
            PatternData = await Task.Run(() => ComputeBeamPatternHeatMap(cancel, _PatternProcessProgressReporter), cancel).ConfigureAwait(true);
        }

        private double[,] ComputeBeamPatternHeatMap(CancellationToken cancel, IProgress<double?> progress)
        {
            const double th_max = 90;
            const double dth = 0.5;
            const int N = 2 * (int)(th_max / dth);
            var data = new double[N, N];

            Antenna antenna = _AntennaArray;
            var f0 = _f0 * c_GHz;
            progress?.Report(0);
            var max = double.NegativeInfinity;

            //var options = new ParallelOptions { CancellationToken = cancel, MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };
            //Parallel.For(0, N, options, i =>
            //{
            //    for (var j = 0; j < N; j++)
            //    {
            //        var angle = new Complex(N / 2 - i, j - N / 2);
            //        var th = angle.Abs * dth;
            //        if (th > 90)
            //        {
            //            data[i, j] = double.NaN;
            //            continue;
            //        }
            //        cancel.ThrowIfCancellationRequested();
            //        var F = antenna.Pattern(th * Consts.ToRad, angle.Arg, f0);
            //        cancel.ThrowIfCancellationRequested();
            //        var f = F.Power.In_dB_byPower();
            //        data[i, j] = f;
            //        if (f > max) max = f;
            //    }
            //    progress?.Report((double)i / N);
            //});

            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < N; j++)
                {
                    var angle = new Complex(N / 2 - i, j - N / 2);
                    var th = angle.Abs * dth;
                    if (th > 90)
                    {
                        data[i, j] = double.NaN;
                        continue;
                    }
                    cancel.ThrowIfCancellationRequested();
                    var F = antenna.Pattern(th * Consts.ToRad, angle.Arg, f0);
                    cancel.ThrowIfCancellationRequested();
                    var f = F.Power.In_dB_byPower();
                    data[i, j] = f;
                    if (f > max) max = f;
                }
                progress?.Report((double)i / N);
            }
            const double min = -80;
            for (var i = 0; i < N; i++)
                for (var j = 0; j < N; j++)
                {
                    data[i, j] -= max;
                    if (data[i, j] < min)
                        data[i, j] = min;

                }

            progress?.Report(cancel.IsCancellationRequested ? 0 : 1);
            cancel.ThrowIfCancellationRequested();
            return data;
        }

        /// <summary>Метод изменения значения прогресса вычисления полного КНД</summary>
        /// <param name="progress">Значение прогресса от 0 до 1 включительно</param>
        private void OnD0ProgressReport(double? progress) => PatternComplete = progress;

        /// <summary>Обработчик событий внутри объекта амплитудного распределения, вызывающий перерасчёт значений амплитудного распределения</summary>
        /// <param name="sender">Источник события - экземпляр амплитудного распределения</param>
        /// <param name="args">Аргумент события, несущий имя изменившегося свойства</param>
        private void OnDistributionPropertyChanged(object sender, PropertyChangedEventArgs args) => SetDestribution();

        /// <summary>Получить пречисление значений аргумента для расчёта функции амплитудного распределения</summary>
        /// <returns></returns>
        private IEnumerable<double> GetXRange()
        {
            var dx = _dx;
            for (var x = -0.5; x < 0.5; x += dx)
                yield return x;
            yield return 0.5;
        }

        /// <summary>Метод асинхронного вычисления массива значений ДН антенной решётки</summary>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Асинхронная задача вычисления значений ДН антенной решётки</returns>
        private Task<PatternValue[]> GetBeam0DataAsync(CancellationToken Cancel)
        {
            const double th_min = -180 * c_ToRad;
            const double th_max = 180 * c_ToRad;
            return Task.Run(() => _AntennaArray.Element.GetPatternValuesParallel(_f0 * c_GHz, _PhiView * c_ToRad, th_min, th_max, _dθ * Consts.ToRad, Cancel), Cancel);
        }

        /// <summary>Метод асинхронного вычисления массива значений ДН элементарного излучателя антенной решётки</summary>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Асинхронная задача вычисления значений ДН элементарного излучателя антенной решётки</returns>
        private Task<PatternValue[]> GetBeamDataAsync(CancellationToken Cancel)
        {
            const double th_min = -180 * c_ToRad;
            const double th_max = 180 * c_ToRad;
            return Task.Run(() => _AntennaArray.GetPatternValuesParallel(_f0 * c_GHz, _PhiView * c_ToRad, th_min, th_max, _dθ * Consts.ToRad, Cancel), Cancel);
        }

        /// <summary>Раcсчитать диаграмму направленности антенны (асинхронно)</summary>
        private async void ComputePatternAsync()
        {
            _ComputePatternCancellationTokenSource?.Cancel();
            _ComputePatternCancellationTokenSource = new CancellationTokenSource();
            var cancel = _ComputePatternCancellationTokenSource.Token;

            try
            {
                //var compute_d0_taks = ComputeD0Async(_AntennaArray.Pattern, _f0 * c_GHz, _D0ProgressReporter, cancel);
                var beam_task = GetBeamDataAsync(cancel);
                var beam0_task = GetBeam0DataAsync(cancel);
                var beam_heatmap_task = ComputeBeamPatternHeatMapAsync(cancel);
                if (cancel.IsCancellationRequested) return;
                var beam_data_points = await beam_task.ConfigureAwait(true);
                var analyses_pattern_task = AnalysisPatternAsync(beam_data_points, cancel);
                if (cancel.IsCancellationRequested) return;
                var beam_0_data_points = await beam0_task.ConfigureAwait(true);
                if (cancel.IsCancellationRequested) return;
                if (cancel.IsCancellationRequested) return;
                var beam0 = beam_0_data_points.ToArray();
                if (cancel.IsCancellationRequested) return;
                BeamData = beam_data_points;
                if (cancel.IsCancellationRequested) return;
                Beam0Data = beam0;
                if (cancel.IsCancellationRequested) return;
                await Task.WhenAll(analyses_pattern_task/*, compute_d0_task*/, beam_heatmap_task).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                //Debug.WriteLine(e);
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>Рассчитать полный КНД трёхмерной ДН антенной решётки</summary>
        /// <param name="F">Функция ДН решётки</param>
        /// <param name="f">Частота вычисления ДН решётки</param>
        /// <param name="Progress">Информатор об изменении прогресса вычисления КНД</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача вычисления полного КНД решётки</returns>
        private async Task ComputeD0Async(Func<double, double, double, Complex> F, double f, IProgress<double?> Progress, CancellationToken Cancel)
        {
            try
            {
                const double th1 = Consts.pi05neg;
                const double th2 = Consts.pi05;
                const double phi1 = 0;
                const double phi2 = Consts.pi2;
                const double da = 1 * Consts.ToRad;
                //const double Dphi = phi2 - phi1;
                const int N_phi = (int)((phi2 - phi1) / da) + 1;
                const int M_th = (int)((th2 - th1) / da) + 1;
                var pattern_data = new double[N_phi, M_th];

                var D0 = await Task.Run(() =>
                {
                    Progress?.Report(null);

                    var d0 = 0d;
                    var max = double.NegativeInfinity;

                    for (var i = 0; i < N_phi; i++)
                    {
                        var phi = i * da + phi1;
                        for (var j = 0; j < M_th; j++)
                        {
                            var th = j * da + th1;
                            Cancel.ThrowIfCancellationRequested();
                            var v = F(th, phi, f).Power;
                            pattern_data[i, j] = v;
                            if (v > max) max = v;
                            var d1 = v * Math.Cos(th);
                            d0 += d1;
                        }
                        Progress?.Report((double)i / N_phi);
                    }

                    Cancel.ThrowIfCancellationRequested();
                    Progress?.Report(1d);
                    return 4 * Math.PI * max * da / d0;
                }, Cancel).ConfigureAwait(true);
                Cancel.ThrowIfCancellationRequested();
                D0_Total = D0.In_dB_byPower();

                PatternData = await Task.Run(() =>
                {
                    var max = pattern_data.Max2D();
                    for (var i = 0; i < N_phi; i++)
                        for (var j = 0; j < M_th; j++)
                            pattern_data[i, j] = (pattern_data[i, j] / max).In_dB_byPower();
                    return pattern_data;
                }, Cancel).ConfigureAwait(true);
            }
            catch (OperationCanceledException e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        /// <summary>Асинхронный прцоцесс анализа ДН антенной решётки</summary>
        /// <param name="pattern">Массив значений рассчитанной ДН</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача асинхронного анализа ДН антенной решётки</returns>
        private async Task AnalysisPatternAsync(PatternValue[] pattern, CancellationToken Cancel)
        {
            var result = await Task.Factory.StartNew(p => ((PatternValue[])p).Analyse(), pattern, Cancel).ConfigureAwait(true);
            MaxPos = result.MaxIndex;
            var gain = result.Max.In_dB();
            Gain = gain;
            D0 = result.D0.In_dB();
            var ssl = result.SSL.In_dB();
            SSL = ssl;
            SSL_Value = ssl - gain;
            var maean_ssl = result.MeanSLL.In_dB();
            MeanSSL = maean_ssl;
            MeanSSL_Value = MeanSSL - gain;

            BeamPos = result.MaxAngle.ToDeg();
            BeamWidth07 = (result.Th07Right - result.Th07Left).ToDeg();
            BeamWidth0 = (result.Th0Right - result.Th0Left).ToDeg();

            BeamLeftAdge07 = result.Th07Left.ToDeg();
            BeamRightAdge07 = result.Th07Right.ToDeg();
        }

        /// <summary>Обработчик событий изменений параметров антенного элемента</summary>
        /// <param name="sender">Источник события - антенный элемент</param>
        /// <param name="args">Аргумент события, несущий имя изменившегося свойства</param>
        private void OnElementaryAntennaChanged(object sender, PropertyChangedEventArgs args) => OnPropertyChanged(nameof(ElementaryAntenna));

        /// <summary>Метод установки амплитудного распределения</summary>
        private void SetDestribution()
        {
            var phase = GetPhaseDistribution(_θ0 * c_ToRad, _φ0 * c_ToRad, _f0 * c_GHz);
            var lx = Lx;
            var ly = Ly;
            var infx = lx.Equals(0);
            var infy = ly.Equals(0);
            var ilx = 1 / lx;
            var ily = 1 / ly;
            if (!infx && !infy)
                _AntennaArray.Distribution = (x, y) => Complex.Exp(_A(x * ilx, y * ily), phase(x, y));
            else if (infx && !infy)
                _AntennaArray.Distribution = (x, y) => Complex.Exp(_A(0, y * ily), phase(0, y));
            else if (!infx && infy)
                _AntennaArray.Distribution = (x, y) => Complex.Exp(_A(x * ilx, 0), phase(x, 0));
            else
            {
                var A = _A(0, 0);
                _AntennaArray.Distribution = (x, y) => A;
            }

            OnPropertyChanged(nameof(DistributionData));
        }

        /// <summary>Получение функции фазового распределения апертуры для заданной частоты</summary>
        /// <param name="Theta">Угол места фазирования антенной решётки</param>
        /// <param name="Phi">Угол азимута фазирования антенной решётки</param>
        /// <param name="f">Частота расчёта фазового распределения</param>
        /// <returns>Функция фазового распределения</returns>
        private static Func<double, double, double> GetPhaseDistribution(double Theta, double Phi, double f)
        {
            var sinTh = Math.Sin(Theta) * (Consts.pi2 / Consts.SpeedOfLight * f);
            var cosPh = Math.Cos(Phi) * sinTh;
            var sinPh = Math.Sin(Phi) * sinTh;
            return (x, y) => x * cosPh + y * sinPh;
        }

        #endregion

        /* ------------------------------------------------------------------------------------------------ */
    }
}
