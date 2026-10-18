using System.Linq.Expressions;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas;

/// <summary>Элемент Гюйгенса - антенный элемент с диаграммой направленности cos²(θ)</summary>
public class Huygens : Antenna
{
    /// <inheritdoc />
    public override Complex Pattern(SpaceAngle Direction, double f)
    {
        var v = Math.Cos(Direction.ThetaRad);
        return v * v;
    }

    /// <inheritdoc />
    public override Expression GetPatternExpressionBody(Expression a, Expression f)
    {
        var cos = MathExpression.Cos(a.GetProperty(nameof(SpaceAngle.ThetaRad)));
        return AsComplex(Expression.Multiply(cos, cos));
    }

    /// <inheritdoc />
    public override string ToString() => "Элемент Гюйгенса";
}

/// <summary>Устаревшее имя элемента Гюйгенса</summary>
[Obsolete("Используйте Huygens")]
public sealed class Guigens : Huygens;