using System.Windows.Data;
using OxyPlot;

namespace ArrayFactor
{
    public partial class MainWindow
    {
        public MainWindow() { InitializeComponent(); }

        private void OnDataUpdated(object Sender, DataTransferEventArgs E)
        {
            Model m = new PlotModel();
            var counturs = new OxyPlot.Series.ContourSeries();

            
        }
    }
}