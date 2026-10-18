namespace Antennas.Calculation;

/// <summary>Неизменяемый снимок входных данных для расчёта диаграммы направленности</summary>
/// <param name="Antenna">Антенная решётка</param>
/// <param name="Element">Элементарный излучатель</param>
/// <param name="Frequency">Частота в герцах</param>
/// <param name="Phi">Азимутальный угол сечения в радианах</param>
/// <param name="ThetaMin">Начальный угол места в радианах</param>
/// <param name="ThetaMax">Конечный угол места в радианах</param>
/// <param name="ThetaStep">Шаг по углу места в радианах</param>
/// <param name="HeatMapSize">Размер стороны тепловой карты</param>
/// <param name="HeatMapAngleStepDeg">Шаг тепловой карты в градусах</param>
/// <param name="HeatMapMinDb">Нижняя граница тепловой карты в дБ</param>
public sealed record PatternRequest(
    Antenna Antenna,
    Antenna Element,
    double Frequency,
    double Phi,
    double ThetaMin,
    double ThetaMax,
    double ThetaStep,
    int HeatMapSize = 360,
    double HeatMapAngleStepDeg = 0.5,
    double HeatMapMinDb = -80);
