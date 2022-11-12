#nullable enable
using System.Linq.Expressions;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;
using MathCore.ViewModels;

namespace Antennas;

/// <summary>Антенна</summary>
public abstract class Antenna : ViewModel, IAntenna
{
    /// <summary>Диаграмма направленности</summary>
    /// <param name="Theta">Угол места</param>
    /// <param name="Phi">Угол азимута</param>
    /// <param name="f">Частота</param>
    /// <returns>Значение диаграммы направленности в указанном направлении</returns>
    public Complex Pattern(double Theta, double Phi, double f) => Pattern(new SpaceAngle(Theta, Phi), f);

    /// <summary>Диаграмма направленности</summary>
    /// <param name="Direction">пространственное направление</param>
    /// <param name="f">Частота</param>
    /// <returns>Значение диаграммы направленности в указанном направлении</returns>
    public abstract Complex Pattern(SpaceAngle Direction, double f);

    /// <summary>Получение диаграммы направленности на указанной частоте</summary>
    /// <param name="f">Частота</param>
    /// <returns>Функция диаграммы направленности от пространственного угла</returns>
    public Func<SpaceAngle, Complex> GetPattern(double f) => a => Pattern(a, f);

    /// <summary>Получение функции диаграммы направленности в зависимости от меридионального угла</summary>
    /// <param name="Phi">Фиксируемый азимутальный</param>
    /// <returns>Значение диаграммы направленности в меридиональных углах</returns>
    public Func<double, double, Complex> GetPatternOfTheta(double Phi = 0) => (Theta, f) => Pattern(new SpaceAngle(Theta, Phi), f);

    /// <summary>Получение функции диаграммы направленности в зависимости от меридионального угла на частоте</summary>
    /// <param name="f">Частота</param>
    /// <param name="Phi">Азимутальный угол</param>
    /// <returns>Значение диаграммы направленности в меридиональных углах на частоте f</returns>
    public Func<double, Complex> GetPatternOfThetaOnFreq(double f, double Phi = 0) => Theta => Pattern(new SpaceAngle(Theta, Phi), f);


    /// <summary>Получение функции диаграммы направленности в зависимости от угла места на частоте</summary>
    /// <param name="Theta">Угол места</param>
    /// <returns>Значение диаграммы направленности в меридиональных углах на частоте f</returns>
    public Func<double, double, Complex> GetPatternOfPhi(double Theta = 0) => (Phi, f) => Pattern(new SpaceAngle(Theta, Phi), f);

    /// <summary>Получение функции диаграммы направленности в зависимости от угла места на частоте</summary>
    /// <param name="f">Частота</param>
    /// <param name="Theta">Угол места</param>
    /// <returns>Значение диаграммы направленности в меридиональных углах на частоте f</returns>
    public Func<double, Complex> GetPatternOfPhiOnFreq(double f, double Theta = 0) => Phi => Pattern(new SpaceAngle(Theta, Phi), f);

    /// <summary>Метод получения тела выражения функции диаграммы направленности для переопределения в классах-наследниках</summary>
    /// <param name="a">Выражение параметра угла</param>
    /// <param name="f">Выражение параметра частоты</param>
    /// <returns>Тело выражения функции диаграммы направленности зависящее от пространственного угла и частоты</returns>
    public virtual Expression? GetPatternExpressionBody(Expression a, Expression f) =>
        ((Func<SpaceAngle, double, Complex>)Pattern).GetCallExpression(a, f);

    public Expression<Func<SpaceAngle, double, Complex>> GetPatternExpression()
    {
        var a = "Angle".ParameterOf(typeof(SpaceAngle));
        var f = "f".ParameterOf(typeof(double));
        return GetPatternExpressionBody(a, f).NotNull()
           .CreateLambda<Func<SpaceAngle, double, Complex>>(a, f);
    }

    public override string ToString() => GetType().Name;
}