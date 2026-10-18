using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service;

[ValueConversion(typeof(double), typeof(string))]
internal class DoubleToStringConverter(int Precision) : MarkupExtension, IValueConverter
{
    public DoubleToStringConverter() : this(0) { }

    public override object? ProvideValue(IServiceProvider s) => this;

    public object? Convert(object? v, Type t, object? p, CultureInfo c) =>
        double.IsInfinity(System.Convert.ToDouble(v))
            ? "-"
            : System.Convert.ToDouble(v).Round(Precision).ToString(CultureInfo.InvariantCulture);

    public object? ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        System.Convert.ToDouble(v, CultureInfo.InvariantCulture);
}