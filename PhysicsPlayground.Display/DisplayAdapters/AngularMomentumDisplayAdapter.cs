using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PhysicsPlayground.Simulation.Simulators;

namespace PhysicsPlayground.Display.DisplayAdapters
{
    internal class AngularMomentumDisplayAdapter : IDisplayAdapter<AngularMomentumSimulationMoment>
    {
        private readonly IScreenParametersProvider _screenParams;

        public AngularMomentumDisplayAdapter(IScreenParametersProvider screenParams)
        {
            _screenParams = screenParams;
        }

        public IEnumerable<Shape> GetDrawables(AngularMomentumSimulationMoment angularMomentumSimulationMoment)
        {
            var radius = angularMomentumSimulationMoment.Disc.Radius;
            var angle = angularMomentumSimulationMoment.Angle;

            Ellipse disc = new Ellipse();
            disc.Height = 2 * (radius * _screenParams.PixelsPerMeter);
            disc.Width = 2 * (radius * _screenParams.PixelsPerMeter);
            var brush = new SolidColorBrush(Colors.Black);
            disc.Fill = brush;
            disc.Stroke = brush;
            disc.StrokeThickness = 1;

            Canvas.SetLeft(disc, (_screenParams.XCenter) - _screenParams.PixelsPerMeter * radius);
            Canvas.SetTop(disc, (_screenParams.YCenter) - _screenParams.PixelsPerMeter * radius);

            var line1 = new Line();
            line1.Stroke = new SolidColorBrush(Colors.Red);
            line1.StrokeThickness = 1;
            var x1 = radius * System.Math.Cos(angle);
            var y1 = radius * System.Math.Sin(angle);
            line1.X2 = _screenParams.XCenter + x1 * _screenParams.PixelsPerMeter;
            line1.Y2 = _screenParams.YCenter - y1 * _screenParams.PixelsPerMeter;
            line1.X1 = _screenParams.XCenter - x1 * _screenParams.PixelsPerMeter;
            line1.Y1 = _screenParams.YCenter + y1 * _screenParams.PixelsPerMeter;

            //var line2 = new Line();
            //line2.Stroke = new SolidColorBrush(Colors.Red);
            //line2.StrokeThickness = 1;
            //var x2 = radius * System.Math.Cos(angle + System.Math.PI / 2);
            //var y2 = radius * System.Math.Sin(angle + System.Math.PI / 2);
            //line2.X2 = _screenParams.XCenter + x2 * _screenParams.PixelsPerMeter;
            //line2.Y2 = _screenParams.YCenter - y2 * _screenParams.PixelsPerMeter;
            //line2.X1 = _screenParams.XCenter - x2 * _screenParams.PixelsPerMeter;
            //line2.Y1 = _screenParams.YCenter + y2 * _screenParams.PixelsPerMeter;

            return new List<Shape>{disc, line1};
        }
    }
}