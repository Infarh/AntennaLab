using ArrayDesigner.ViewModels;

using MathCore.Hosting.WPF;

namespace ArrayDesigner;

internal sealed class ServiceLocator : ServiceLocatorHosted
{
    public MainWindowViewModel MainModel => GetRequiredService<MainWindowViewModel>();
}
