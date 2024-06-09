using System.Linq.Expressions;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas;

public class LambdaAntenna(Func<double, double, double, double> Beam) : Antenna
{
    /// <summary>Диаграмма направленности</summary>
    /// <param name="Direction">Пространственное направление</param>
    /// <param name="f">Частота</param>
    /// <returns>Значение диаграммы направленности в указанном направлении</returns>
    public override Complex Pattern(SpaceAngle Direction, double f) => Beam(Direction.ThetaRad, Direction.PhiRad, f);

    public override Expression GetPatternExpressionBody(Expression a, Expression f) =>
        Beam.GetCallExpression(a.GetProperty(nameof(SpaceAngle.ThetaRad)), a.GetProperty(nameof(SpaceAngle.PhiRad)), f);
}