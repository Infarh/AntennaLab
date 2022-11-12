#nullable enable
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArrayFactor.Service.Converters;

[ValueConversion(typeof(double), typeof(double))]
internal class Add : MarkupExtension, IValueConverter
{
    public double B { get; set; }

    public Add() { }
    public Add(double b) => B = b;

    public override object? ProvideValue(IServiceProvider sp) => this;

    public object? Convert(object? v, Type t, object? p, CultureInfo c) => (double)v + B;

    public object? ConvertBack(object? v, Type t, object? p, CultureInfo c) => (double)v - B;
}