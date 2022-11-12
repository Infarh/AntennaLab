#nullable enable
using System.Linq.Expressions;
using MathCore;
using MathCore.Vectors;

namespace Antennas;

public sealed class UniformAntenna : Antenna
{
    public override Complex Pattern(SpaceAngle Direction, double f) => Complex.Real;

    public override Expression GetPatternExpressionBody(Expression a, Expression f) => Complex.Real.ToExpression();

    public override string ToString() => "Всенаправленная антенна";
}