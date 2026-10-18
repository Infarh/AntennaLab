#nullable enable

using MathCore;
using MathCore.Vectors;

namespace Antennas.Tests;

/// <summary>Модульные тесты построения и компиляции выражений диаграмм направленности</summary>
[TestClass]
public class ExpressionTests
{
    private const double __Frequency = 1e9;

    private static void AssertExpressionMatchesPattern(Antenna antenna, params double[] angles)
    {
        var compiled = antenna.GetPatternExpression().Compile();

        foreach (var theta in angles)
        {
            var direction = new SpaceAngle(theta, 0.3);
            var expected = antenna.Pattern(direction, __Frequency);
            var actual = compiled(direction, __Frequency);

            Assert.AreEqual(expected.Re, actual.Re, 1e-9, $"Re не совпадает для {antenna.GetType().Name} при θ={theta}");
            Assert.AreEqual(expected.Im, actual.Im, 1e-9, $"Im не совпадает для {antenna.GetType().Name} при θ={theta}");
        }
    }

    /// <summary>Выражение всенаправленной антенны должно компилироваться</summary>
    [TestMethod]
    public void UniformAntenna_Expression_MatchesPattern() =>
        AssertExpressionMatchesPattern(new UniformAntenna(), 0, 0.5, 1.2);

    /// <summary>Выражение диполя должно компилироваться и совпадать с Pattern</summary>
    [TestMethod]
    public void Dipole_Expression_MatchesPattern() =>
        AssertExpressionMatchesPattern(new Dipole(), 0, 0.5, 1.2, 2.5);

    /// <summary>Выражение элемента Гюйгенса должно компилироваться и совпадать с Pattern</summary>
    [TestMethod]
    public void Huygens_Expression_MatchesPattern() =>
        AssertExpressionMatchesPattern(new Huygens(), 0, 0.5, 1.2, 2.5);

    /// <summary>Выражение вибратора должно компилироваться и совпадать с Pattern</summary>
    [TestMethod]
    public void Vibrator_Expression_MatchesPattern() =>
        AssertExpressionMatchesPattern(new Vibrator(0.15), 0.1, 0.5, 1.2, 2.5);

    /// <summary>Выражение пользовательской антенны должно компилироваться и совпадать с Pattern</summary>
    [TestMethod]
    public void LambdaAntenna_Expression_MatchesPattern() =>
        AssertExpressionMatchesPattern(new LambdaAntenna((theta, phi, _) => Math.Cos(theta) + 0.5 * Math.Sin(phi)), 0.1, 0.5, 1.2);

    /// <summary>Выражение антенного элемента должно компилироваться и совпадать с Pattern</summary>
    [TestMethod]
    public void AntennaItem_Expression_MatchesPattern()
    {
        var item = new AntennaItem(new Dipole(), new Vector3D(0.1, 0.05, 0), SpaceAngle.k, new Complex(0.7, 0.3));

        AssertExpressionMatchesPattern(item, 0.1, 0.5, 1.2);
    }

    /// <summary>Выражение повёрнутого антенного элемента должно компилироваться и совпадать с Pattern</summary>
    [TestMethod]
    public void AntennaItem_WithRotation_Expression_MatchesPattern()
    {
        var item = new AntennaItem(new Dipole(), new Vector3D(0.1, 0, 0), new SpaceAngle(0.2, 0.1), new Complex(0.7, 0.3));

        AssertExpressionMatchesPattern(item, 0.1, 0.5, 1.2);
    }

    /// <summary>Выражение линейной решётки должно компилироваться и совпадать с Pattern</summary>
    [TestMethod]
    public void LinearAntennaArray_Expression_MatchesPattern()
    {
        var array = new LinearAntennaArray(4, 0.15);

        AssertExpressionMatchesPattern(array, -0.5, 0, 0.5);
    }
}
