#nullable enable
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Converters;

[ValueConversion(typeof(double), typeof(double))]
internal class Divide : MarkupExtension, IValueConverter
{
    public double K { get; set; }

    public Divide() { K         = 1; }
    public Divide(double k) { K = k; }

    public override object? ProvideValue(IServiceProvider sp) => this;

    public object? Convert(object? v, Type t, object? p, CultureInfo c) => (double)v / K;

    public object? ConvertBack(object? v, Type t, object? p, CultureInfo c) => (double)v * K;
}