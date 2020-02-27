using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PhysicsPlayground.Display.DisplayAdapters
{
    class PointObjectsToEllipseAdapter : IDisplayAdapter<IEnumerable<(double, double)>>
    {
        private readonly IScreenParametersProvider _screenParameters;
        private readonly double _radius;

        public PointObjectsToEllipseAdapter(IScreenParametersProvider screenParameters, double radius = 0.5)
        {
            _screenParameters = screenParameters;
            _radius = radius;
        }

        public IEnumerable<Shape> GetDrawables(IEnumerable<(double, double)> adaptable)
        {
            return adaptable.Select(coordinate =>
            {
                var (x, y) = coordinate;
                
                Ellipse ball = new Ellipse();
                ball.Height = 2 * (_radius * _screenParameters.PixelsPerMeter);
                ball.Width = 2 * (_radius * _screenParameters.PixelsPerMeter);
                ball.Fill = new SolidColorBrush(Colors.Black);

                Canvas.SetLeft(ball, (_screenParameters.XCenter + x * _screenParameters.PixelsPerMeter) - _screenParameters.PixelsPerMeter * _radius);
                Canvas.SetTop(ball, (_screenParameters.YCenter - y * _screenParameters.PixelsPerMeter) - _screenParameters.PixelsPerMeter * _radius);

                return ball;
            });
        }
    }
}