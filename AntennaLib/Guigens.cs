using System;
using System.Linq.Expressions;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas
{
    public class Guigens : Antenna
    {
        /// <inheritdoc />
        public override Complex Pattern(SpaceAngle Direction, double f)
        {
            var v = Math.Cos(Direction.ThetaRad);
            return v * v;
        }

        /// <inheritdoc />
        public override Expression GetPatternExpressionBody(Expression a, Expression f) => 
            MathExpression.Cos(a.GetProperty(nameof(SpaceAngle.ThetaRad)).Divide(2.ToExpression())).Power(2.ToExpression());

        /// <inheritdoc />
        public override string ToString() => "Элемент Гюйгенса";
    }
}