#nullable enable
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Shapes;

[ValueConversion(typeof(double), typeof(double)), MarkupExtensionReturnType(typeof(AngleArcProgressBarConverter))]
public class AngleArcProgressBarConverter : MarkupExtension, IMultiValueConverter
{
    public override object? ProvideValue(IServiceProvider s) => this;

    public object Convert(object[] vv, Type t, object p, CultureInfo c)
    {
        if (vv?.Length != 3) throw new ArgumentException("Требуется три параметра: минимальное значение, текущее и максимальное значение");

        var min   = System.Convert.ToDouble(vv[0]);
        var value = System.Convert.ToDouble(vv[0]);
        var max   = System.Convert.ToDouble(vv[0]);

        var d = max - min;
        var k = value - min;

        return 180 * k / d;
    }

    public object[] ConvertBack(object v, Type[] t, object p, CultureInfo c) => throw new NotSupportedException();
}