#nullable enable
using System.Linq.Expressions;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas;

public class Dipole : Antenna
{
    /// <inheritdoc />
    public override Complex Pattern(SpaceAngle Direction, double f) => Math.Cos(Direction.ThetaRad);

    /// <inheritdoc />
    public override Expression GetPatternExpressionBody(Expression a, Expression f) => MathExpression.Cos(a.GetProperty(nameof(SpaceAngle.ThetaRad)));

    /// <inheritdoc />
    public override string ToString() => "Диполь";
}