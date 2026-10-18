namespace Antennas.Calculation;

/// <summary>Результат расчёта диаграммы направленности</summary>
/// <param name="ArrayPattern">Сечение ДН антенной решётки</param>
/// <param name="ElementPattern">Сечение ДН элементарного излучателя</param>
/// <param name="HeatMap">Тепловая карта пространственной ДН</param>
/// <param name="Analysis">Результат анализа сечения ДН</param>
public sealed record PatternResult(
    PatternValue[] ArrayPattern,
    PatternValue[] ElementPattern,
    double[,] HeatMap,
    AnalysePatternResult Analysis);
