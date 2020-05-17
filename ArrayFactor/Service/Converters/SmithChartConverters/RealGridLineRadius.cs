using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Converters.SmithChartConverters
{
    [ValueConversion(typeof(double), typeof(double))]
    internal class RealGridLineRadius : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider sp) => this;

        public object Convert(object v, Type t, object p, CultureInfo c) => 1 / ((double)v + 1);

        public object ConvertBack(object v, Type t, object p, CultureInfo c) => (1 / (double)v - 1);
    }
}
