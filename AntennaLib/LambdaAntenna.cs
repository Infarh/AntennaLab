using System;
using System.Linq.Expressions;
using MathCore;
using MathCore.Annotations;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas
{
    public class LambdaAntenna : Antenna
    {
        [NotNull]
        private readonly Func<double, double, double, double> _Beam;

        public LambdaAntenna([NotNull]Func<double, double, double, double> Beam) => _Beam = Beam;

        /// <summary>Диаграмма направленности</summary>
        /// <param name="Direction">пространственное направление</param>
        /// <param name="f">Частота</param>
        /// <returns>Значение диаграммы направленности в указанном направлении</returns>
        public override Complex Pattern(SpaceAngle Direction, double f) => _Beam(Direction.ThetaRad, Direction.PhiRad, f);

        public override Expression GetPatternExpressionBody(Expression a, Expression f) =>
            _Beam.GetCallExpression(a.GetProperty(nameof(SpaceAngle.ThetaRad)), a.GetProperty(nameof(SpaceAngle.PhiRad)), f);
    }
}