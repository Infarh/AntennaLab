using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Converters.SmithChartConverters;

internal class RealGridLineOffset : MarkupExtension, IMultiValueConverter
{
    public override object? ProvideValue(IServiceProvider sp) => this;

    public object Convert(object[] vv, Type t, object p, CultureInfo c)
    {
        var h = (double)vv[0];
        var R = (double)vv[1];

        var r = R / (1 + R);

        return h / 2 * r;
    }

    public object[] ConvertBack(object v, Type[] t, object p, CultureInfo c) => throw new NotSupportedException();
}