using MathCore.WPF.ViewModels;

namespace ArrayDesigner.ViewModels;

internal class LayoutDesignerViewModel(MainWindowViewModel MainModel) : ViewModel
{
    public MainWindowViewModel MainModel { get; } = MainModel;

    public double Lx { get; set => Set(ref field, value); } = 100;

    public double Ly { get; set => Set(ref field, value); } = 100;

    public int Nx { get; set => Set(ref field, value); } = 4;

    public int Ny { get; set => Set(ref field, value); } = 4;
}
