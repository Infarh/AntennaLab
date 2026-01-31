using MathCore;

// ReSharper disable once CheckNamespace
namespace Antennas;

/// <summary>Значение диаграммы направленности в одной точке</summary>
public readonly struct PatternValue(double angle, Complex v)
{
    /// <summary>Угол в радианах</summary>
    public double Angle { get; } = angle;
    
    /// <summary>Комплексное значение диаграммы направленности</summary>
    public Complex Value { get; } = v;

    /// <summary>Угол в градусах</summary>
    public double AngleDeg => Angle * Consts.ToDeg;
    
    /// <summary>Угол в радианах (синоним Angle)</summary>
    public double AngleRad => Angle * Consts.ToRad;
    
    /// <summary>Значение диаграммы направленности в дБ</summary>
    public double ValueIndB => Value.Abs.In_dB();
    
    /// <summary>Значение диаграммы направленности в дБ по мощности</summary>
    public double ValueIndBP => Value.Abs.In_dB_byPower();
    
    /// <summary>Модуль значения диаграммы направленности</summary>
    public double ValueAbs => Value.Abs;

    /// <inheritdoc />
    public override string ToString() => $"{AngleDeg:0.00}:{ValueIndB:0.##}db";
}