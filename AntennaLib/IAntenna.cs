using MathCore;
using MathCore.Vectors;

namespace Antennas
{
    /// <summary>Объект по свойствам похожий на антенну</summary>
    public interface IAntenna
    {
        /// <summary>Диаграмма направленности антенны</summary>
        /// <param name="Direction">Пространственный угол</param>
        /// <param name="f">Частота</param>
        /// <returns>Комплексное значение диаграммы направленности</returns>
        Complex Pattern(SpaceAngle Direction, double f);
    }
}