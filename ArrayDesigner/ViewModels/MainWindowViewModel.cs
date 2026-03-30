using System.Windows.Markup;

using MathCore.DI;
using MathCore.WPF.ViewModels;

namespace ArrayDesigner.ViewModels;

[Service]
[MarkupExtensionReturnType(typeof(MainWindowViewModel))]
internal class MainWindowViewModel : TitledViewModel
{
    public MainWindowViewModel() : base("Антенная решётка")
    {
        var layout = LayoutDesigner;
        var nx = layout.Nx;
        var ny = layout.Ny;
        var dx = layout.Lx / (layout.Nx - 1);
        var dy = layout.Ly / (layout.Ny - 1);
        var items = Enumerable.Range(0, /*nx * ny*/ 1)
            .Select(i => new ArrayItemViewModel
            {
                Index = i + 1,
                //X = i % nx * dx,
                //Y = i / nx * dy,
                X = 50,
                Y = 50,
            });
        Items = [.. items];
    }

    public LayoutDesignerViewModel LayoutDesigner => field ??= new(this);

    public string? Status { get; set => Set(ref field, value); } = "Готов!";

    public IReadOnlyList<ArrayItemViewModel> Items { get; private set => Set(ref field!, value); }
}
