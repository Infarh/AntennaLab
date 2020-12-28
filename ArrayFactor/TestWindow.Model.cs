using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;

namespace ArrayFactor
{
    [MarkupExtensionReturnType(typeof(TestWindowModel))]
    internal class TestWindowModel : ViewModel
    {
        private static double DefaultComputeFunction(double x) => double.IsNaN(x) ? double.NaN : x.Equals(0) ? 1 : Math.Sin(Math.PI * x) / (Math.PI * x);
        private static double ComputeFunction(double x)
        {
            System.Threading.Thread.Sleep(10);
            return DefaultComputeFunction(x);
        }

        public double x1 { get => Get<double>(); private set => Set(value); }
        public double x2 { get => Get<double>(); private set => Set(value); }

        public FunctionManager<double> Manager { get; }

        public ICommand DecreseEpsCommand { get; }
        public ICommand ResetCommand { get; }

        public ProgressInfo ComputeProgress { get; } = new();

        public TestWindowModel()
        {
            x1 = -10;
            x2 = 10;
            Manager = new FunctionManager<double>(ComputeFunction, x1, x2, v => v);
            Manager.SetEps(10);
            DecreseEpsCommand = new LambdaCommand(DecreaseEpsAsync, CanDecreaseEps);
            ResetCommand = new LambdaCommand(o => Manager.Reset());
        }

        private bool CanDecreaseEps(object Arg) => Manager.Eps0 > 1e-7;

        private CancellationTokenSource _DecreaseEpsCancellationTokenSource = new();
        private async void DecreaseEpsAsync()
        {
            _DecreaseEpsCancellationTokenSource.Cancel();
            _DecreaseEpsCancellationTokenSource = new CancellationTokenSource();
            var cancel = _DecreaseEpsCancellationTokenSource.Token;
            try
            {
                await Manager.SetEpsAsync(Manager.Eps0 / 10, ComputeProgress, cancel).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
