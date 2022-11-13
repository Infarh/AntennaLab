using MathCore;
using MathCore.WPF.ViewModels;

namespace ArrayFactor;

public sealed class WaveInfo : ViewModel
{
    private double _Frequency = 1e9;
    private double _Bandwidth = 0.015;
    private double _Length = 30e-2;
    private readonly object _SyncRoot = new();

    public double Frequency
    {
        get => _Frequency;
        set
        {
            if(_Frequency.Equals(value)) return;
            lock(_SyncRoot)
            {
                if(_Frequency.Equals(value)) return;
                _Frequency = value;
                _Length    = Consts.SpeedOfLight / value;
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(k));
        }
    }

    public double Length
    {
        get => _Length;
        set
        {
            if(_Length.Equals(value)) return;
            lock(_SyncRoot)
            {
                if(_Length.Equals(value)) return;
                _Length    = value;
                _Frequency = Consts.SpeedOfLight / value;
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(Frequency));
            OnPropertyChanged(nameof(k));
        }
    }

    public double k => Consts.pi2 / Length;

    public double Bandwidth
    {
        get => _Bandwidth;
        set
        {
            if(_Bandwidth.Equals(value)) return;
            _Bandwidth = value;
            OnPropertyChanged();
        }
    }
}