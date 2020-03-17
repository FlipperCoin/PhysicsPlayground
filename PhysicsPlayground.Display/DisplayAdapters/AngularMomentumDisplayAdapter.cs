using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PhysicsPlayground.Simulation.Simulator.Implementations;

namespace PhysicsPlayground.Display.DisplayAdapters
{
    internal class AngularMomentumDisplayAdapter : IDisplayAdapter<AngularMomentumSimulationMoment>
    {
        private readonly IScreenParametersProvider _screenParams;
        private readonly bool _floor;
        private readonly bool _move;

        public AngularMomentumDisplayAdapter(IScreenParametersProvider screenParams, bool floor, bool move)
        {
            _screenParams = screenParams;
            _floor = floor;
            _move = move;
        }

        public IEnumerable<Shape> GetDrawables(AngularMomentumSimulationMoment angularMomentumSimulationMoment)
        {
            var shapes = new List<Shape>();

            var radius = angularMomentumSimulationMoment.Disk.Radius;
            var angle = angularMomentumSimulationMoment.Angle;

            if (_floor)
            {
                var floorShape = new Line();
                var yFloor = -radius;
                floorShape.X1 = 0;
                floorShape.X2 = _screenParams.Width;
                floorShape.Y1 = _screenParams.YCenter - yFloor * _screenParams.PixelsPerMeter;
                floorShape.Y2 = _screenParams.YCenter - yFloor * _screenParams.PixelsPerMeter;
                floorShape.StrokeThickness = 2;
                floorShape.Stroke = new SolidColorBrush(Colors.Gray);
                shapes.Add(floorShape);
            }

            Ellipse disc = new Ellipse();
            disc.Height = 2 * (radius * _screenParams.PixelsPerMeter);
            disc.Width = 2 * (radius * _screenParams.PixelsPerMeter);
            var brush = new SolidColorBrush(Colors.Black);
            disc.Fill = brush;
            disc.Stroke = brush;
            disc.StrokeThickness = 1;

            var x = 0D;
            if (_move)
            {
                x = angularMomentumSimulationMoment.X.D;
            }

            Canvas.SetLeft(disc, (_screenParams.XCenter) + _screenParams.PixelsPerMeter * x - _screenParams.PixelsPerMeter * radius);
            Canvas.SetTop(disc, (_screenParams.YCenter) - _screenParams.PixelsPerMeter * radius);

            var line1 = new Line();
            line1.Stroke = new SolidColorBrush(Colors.Red);
            line1.StrokeThickness = 1;
            var x1 = radius * System.Math.Cos(angle);
            var y1 = radius * System.Math.Sin(angle);
            line1.X2 = _screenParams.XCenter + x1 * _screenParams.PixelsPerMeter + x * _screenParams.PixelsPerMeter;
            line1.Y2 = _screenParams.YCenter - y1 * _screenParams.PixelsPerMeter;
            line1.X1 = _screenParams.XCenter - x1 * _screenParams.PixelsPerMeter + x * _screenParams.PixelsPerMeter;
            line1.Y1 = _screenParams.YCenter + y1 * _screenParams.PixelsPerMeter;

            shapes.Add(disc);
            shapes.Add(line1);
            return shapes;
        }
    }
}