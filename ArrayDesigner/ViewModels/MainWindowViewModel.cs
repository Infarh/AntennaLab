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

    }
}
