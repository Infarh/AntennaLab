using MathCore.WPF.ViewModels;

namespace ArrayDesigner.ViewModels;

internal class ArrayItemViewModel : ViewModel
{
    public int Index { get; init; }

    public double X { get; set => Set(ref field, value); }

    public double Y { get; set => Set(ref field, value); }
}
