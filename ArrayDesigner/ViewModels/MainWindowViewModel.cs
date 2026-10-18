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
        Items = [new ArrayItemViewModel { Index = 1, X = 50, Y = 50 }];
    }

    public LayoutDesignerViewModel LayoutDesigner => field ??= new(this);

    public string? Status { get; set => Set(ref field, value); } = "Готов!";

    public IReadOnlyList<ArrayItemViewModel> Items { get; private set => Set(ref field!, value); }
}
