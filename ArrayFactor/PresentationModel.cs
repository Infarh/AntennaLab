using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Antennas;
using ArrayFactor.View.Models;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;

namespace ArrayFactor
{
    internal sealed class PresentationModel : ViewModel
    {
        private ICommand _GoToPage;
        private ICommand _GoToIndexPage;
        private ICommand _GoToPreviousPage;
        private ICommand _GoToNextPage;

        private PageModel _CurrentModel;

        private readonly List<PageModel> _PagesSequence;
        private int _CurrentModelIndex = 1;

        private readonly Random _Random = new();
        private AntennaArray _AntennaArray;
        private readonly WaveInfo _Wave = new();

        #region Команды переходов

        public ICommand GoToPage
        {
            get => _GoToPage;
            set => Set(ref _GoToPage, value);
        }

        public ICommand GoToIndexPage
        {
            get => _GoToIndexPage;
            set => Set(ref _GoToIndexPage, value);
        }

        public ICommand GoToPreviousPage
        {
            get => _GoToPreviousPage;
            set => Set(ref _GoToPreviousPage, value);
        }

        public ICommand GoToNextPage
        {
            get => _GoToNextPage;
            set => Set(ref _GoToNextPage, value);
        }

        #endregion


        #region Модели страниц

        public int CurrentModelIndex
        {
            get => _CurrentModelIndex;
            set
            {
                if(_CurrentModelIndex == value || value < 0 || value >= _PagesSequence.Count) return;
                _CurrentModelIndex = value;
                OnPropertyChanged();
            }
        }

        private readonly object _ModelChangeSyncRoot = new();
        public PageModel CurrentModel
        {
            get => _CurrentModel;
            set
            {
                lock(_ModelChangeSyncRoot)
                {
                    if(ReferenceEquals(_CurrentModel, value)) return;
                    _CurrentModel?.OnDeactivated();
                    _CurrentModel = value;
                    _CurrentModel?.OnActivated();
                    OnPropertyChanged();
                }
            }
        }

        public IndexPageModel IndexModel { get; }

        public WelcomePageModel WelcomeModel { get; }

        public RegistrationPageModel RegistrationModel { get; }

        public IntroductionPageModel IntroductionModel { get; }

        public BeamPatternParametersPageModel BeamPatternParametersModel { get; }
        public RawDataPageModel RawDataModel { get; }
        public ArrayGeometryPageModel ArrayGeometryModel { get; }

        #endregion

        //internal StudentsDataSet DataBase => _DataBase;

        public WaveInfo Wave => _Wave;

        public string Title => "Исследование множителя антенной решётки";

        public string Target =>
            "Знакомство и закрепление теоретических сведений о методах " +
            "формирования направленных свойств фазированных антенных решёток";


        public PresentationModel()
        {

            #region Инициализация моделей страниц

            IndexModel = new IndexPageModel(this);
            _CurrentModel = WelcomeModel = new WelcomePageModel(this);
            RegistrationModel = new RegistrationPageModel(this);
            IntroductionModel = new IntroductionPageModel(this);
            BeamPatternParametersModel = new BeamPatternParametersPageModel(this);
            RawDataModel = new RawDataPageModel(this);
            ArrayGeometryModel = new ArrayGeometryPageModel(this);

            _PagesSequence = new List<PageModel>
            {
                IndexModel,
                WelcomeModel,
                RegistrationModel,
                IntroductionModel,
                BeamPatternParametersModel,
                RawDataModel,
                ArrayGeometryModel,
            };

            #endregion

            #region Определения взаимосвязей между свойствами

            this.RegisterPropertyChangedHandler(
                (s, e) => CurrentModel = _PagesSequence[_CurrentModelIndex],
                nameof(CurrentModelIndex));
            this.RegisterPropertyChangedHandler(
                (s, e) => CurrentModelIndex = _PagesSequence.IndexOf(_CurrentModel),
                nameof(CurrentModel));
            _Wave.RegisterPropertyChangedHandler
                (
                    (s, e) => OnPropertyChanged(nameof(Wave)),
                    nameof(WaveInfo.Frequency),
                    nameof(WaveInfo.Length),
                    nameof(WaveInfo.k),
                    nameof(WaveInfo.Bandwidth)
                );

            #endregion


            #region Инициализация команд

            _GoToPage = new LambdaCommand(o => CurrentModelIndex = (int)(o as int? ?? o as double? ?? (o as string).AsInt32() ?? 0));
            _GoToPreviousPage = new LambdaCommand(() =>
            {
                var index = CurrentModelIndex - 1;
                CurrentModelIndex = index < 0 ? _PagesSequence.Count - 1 : index;
            });
            _GoToNextPage = new LambdaCommand(() => CurrentModelIndex = (CurrentModelIndex + 1) % _PagesSequence.Count);
            _GoToIndexPage = new LambdaCommand(() => CurrentModelIndex = 0);

            #endregion

            Wave.Frequency = RandomValue(1, 3, 5, 7, 8, 12, 18) * 1e9;
            Wave.Bandwidth = RandomValue(0.5, 1, 1.5, 2, 2.5, 3) / 100;

            var Nx = RandomIntValue(10, 20);
            var Ny = RandomIntValue(3, 7);
            var dx = Wave.Length / 2;
            var dy = dx;

            _AntennaArray = new RectangularAntennaArray(Nx, Ny, dx, dy, new UniformAntenna(), (x, y) => 1);
        }

        internal double RandomDoubleValue(double M, double D) => (_Random.NextDouble() - 0.5) * D + M;
        internal int RandomIntValue(int To) => _Random.Next(To);
        internal int RandomIntValue(int From, int To) => _Random.Next(From, To);
        internal T RandomValue<T>(params T[] values) => values[RandomIntValue(values.Length - 1)];
    }
}
