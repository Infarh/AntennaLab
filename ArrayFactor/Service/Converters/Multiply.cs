using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Converters;

[ValueConversion(typeof(double), typeof(double))]
internal class Multiply(double k) : MarkupExtension, IValueConverter
{
    public Multiply() : this(1) { }

    public double K { get; set; } = k;

    public override object? ProvideValue(IServiceProvider sp) => this;

    public object? Convert(object? v, Type t, object? p, CultureInfo c) => (double)v! * K;

    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) => (double)value! / K;
}