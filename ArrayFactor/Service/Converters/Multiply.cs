#nullable enable
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Converters;

[ValueConversion(typeof(double), typeof(double))]
internal class Multiply : MarkupExtension, IValueConverter
{
    public double K { get; set; }

    public Multiply() => K = 1;

    public Multiply(double k) => K = k;

    public override object? ProvideValue(IServiceProvider sp) => this;

    public object? Convert(object? v, Type t, object? p, CultureInfo c) => (double)v * K;

    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) => (double)value / K;
}