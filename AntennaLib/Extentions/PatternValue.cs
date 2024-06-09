using MathCore;

// ReSharper disable once CheckNamespace
namespace Antennas;

public readonly struct PatternValue(double angle, Complex v)
{
    public double Angle { get; } = angle;
    public Complex Value { get; } = v;

    public double AngleDeg => Angle * Consts.ToDeg;
    public double AngleRad => Angle * Consts.ToRad;
    public double ValueIndB => Value.Abs.In_dB();
    public double ValueIndBP => Value.Abs.In_dB_byPower();
    public double ValueAbs => Value.Abs;

    /// <inheritdoc />
    public override string ToString() => $"{AngleDeg:0.00}:{ValueIndB:0.##}db";
}