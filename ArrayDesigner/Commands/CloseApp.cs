using System.Windows;

using MathCore.WPF.Commands;

namespace ArrayDesigner.Commands;

internal class CloseApp : Command
{
    public override void Execute(object? parameter)
    {
        Application.Current.Shutdown();
    }
}
