#nullable enable
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Converters.SmithChartConverters;

internal class ImagenGridLineXOffset : MarkupExtension, IMultiValueConverter
{
    public override object? ProvideValue(IServiceProvider sp) => this;

    public object Convert(object[] vv, Type t, object p, CultureInfo c)
    {
        var w = (double)vv[0];
        var X = (double)vv[1];

        return w / 2 / X;
    }

    public object[] ConvertBack(object v, Type[] tt, object p, CultureInfo c) => throw new NotSupportedException();
}