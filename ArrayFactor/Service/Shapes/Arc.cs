#nullable enable
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Math;

namespace ArrayFactor.Service.Shapes;

internal class Arc : Shape
{
    public static readonly DependencyProperty StartAngleProperty = 
        DependencyProperty.Register(
            nameof(StartAngle),
            typeof(double),
            typeof(Arc),
            new UIPropertyMetadata(default(double), (d, _) => ((Arc)d).InvalidateVisual()));

    public double StartAngle { get => (double)GetValue(StartAngleProperty); set => SetValue(StartAngleProperty, value); }

    public static readonly DependencyProperty EndAngleProperty = 
        DependencyProperty.Register(
            nameof(EndAngle),
            typeof(double),
            typeof(Arc),
            new UIPropertyMetadata(90d, (d, _) => ((Arc)d).InvalidateVisual()));

    public double EndAngle { get => (double)GetValue(EndAngleProperty); set => SetValue(EndAngleProperty, value); }

    protected override Geometry DefiningGeometry => GetArcGeometry();

    protected override void OnRender(DrawingContext d) => d.DrawGeometry(null, new Pen(Stroke, StrokeThickness), GetArcGeometry());

    private Geometry GetArcGeometry()
    {
        var angle_start = StartAngle;
        var angle_end   = EndAngle;
        var start       = PointAt(Min(angle_start, angle_end));
        var end         = PointAt(Max(angle_start, angle_end));

        var stroke_th   = StrokeThickness;
        var render_size = RenderSize;
        var size        = new Size(Max(0, (render_size.Width - stroke_th) / 2), Max(0, (render_size.Height - stroke_th) / 2));
        var is_large    = Abs(angle_end - angle_start) > 180;

        var geometry = new StreamGeometry();
        using(var context = geometry.Open())
        {
            context.BeginFigure(start, false, false);
            context.ArcTo(end, size, 0, is_large, SweepDirection.Counterclockwise, true, false);
        }
        geometry.Transform = new TranslateTransform(stroke_th / 2, stroke_th / 2);
        return geometry;
    }

    private Point PointAt(double angle) => new((RenderSize.Width - StrokeThickness) / 2 * (1 + Cos(angle * (PI / 180))), (RenderSize.Height - StrokeThickness) / 2 * (1 - Sin(angle * (PI / 180))));
}