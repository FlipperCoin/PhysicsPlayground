using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PhysicsPlayground.Simulation;
using PhysicsPlayground.Simulation.Simulators;

namespace PhysicsPlayground.Display.DisplayAdapters
{
    internal class ElasticCollisionDisplayAdapter : IDisplayAdapter<ElasticCollisionMoment>
    {
        private readonly IScreenParametersProvider _screenParams;

        public ElasticCollisionDisplayAdapter(IScreenParametersProvider screenParams)
        {
            _screenParams = screenParams;
        }

        public IEnumerable<Shape> GetDrawables(ElasticCollisionMoment adaptable)
        {
            var (box, balls) = (adaptable.EllasticCollisionMetadata.Box, adaptable.Balls);
            List<Shape> shapes = adaptable.Balls.Select(ellipseInPlane =>
            {
                var (massEllipse, (x, y)) = 
                    (ellipseInPlane.Item1, 
                        (ellipseInPlane.Item2.EllipseMovementParameters.X.D, 
                            ellipseInPlane.Item2.EllipseMovementParameters.Y.D)
                        );

                Ellipse ball = new Ellipse();
                ball.Height = 2 * (massEllipse.Radius * _screenParams.PixelsPerMeter);
                ball.Width = 2 * (massEllipse.Radius * _screenParams.PixelsPerMeter);
                var brush = new SolidColorBrush(MassToColor(massEllipse.Mass));
                ball.Fill = brush;
                ball.Stroke = brush;
                ball.StrokeThickness = 1;

                Canvas.SetLeft(ball, (_screenParams.XCenter + x * _screenParams.PixelsPerMeter) - _screenParams.PixelsPerMeter * massEllipse.Radius);
                Canvas.SetTop(ball, (_screenParams.YCenter - y * _screenParams.PixelsPerMeter) - _screenParams.PixelsPerMeter * massEllipse.Radius);

                return ball;
            }).Cast<Shape>().ToList();

            var boxShape = new Rectangle();
            boxShape.Width = (box.X2 - box.X1) * _screenParams.PixelsPerMeter;
            boxShape.Height = (box.Y2 - box.Y1) * _screenParams.PixelsPerMeter;
            boxShape.Stroke = new SolidColorBrush(Colors.Black);
            boxShape.StrokeThickness = 3;

            Canvas.SetLeft(boxShape, (_screenParams.XCenter + box.X1 * _screenParams.PixelsPerMeter));
            Canvas.SetTop(boxShape, _screenParams.YCenter + box.Y1 * _screenParams.PixelsPerMeter);

            shapes.Add(boxShape);

            return shapes;
        }

        private Color MassToColor(double mass)
        {
            var min = 1;
            var max = 100;

            mass = mass >= min ? mass
                    : mass <= max ? mass
                    : max;

            var mOnScale = (mass - min) / (max - min);

            Color from = Colors.Blue;
            Color to = Colors.DarkRed;

            var mColor = MColor(mOnScale, from, to);

            return mColor;
        }

        private static Color MColor(double scaleIndex, Color fromColor, Color toColor)
        {
            var scaledWhite = new[] {fromColor.R, fromColor.G, fromColor.B}.Select(val => (byte) (val * (1 - scaleIndex)));
            var scaledRed = new[] {toColor.R, toColor.G, toColor.B}.Select(val => (byte) (val * scaleIndex));
            var mColorRGB = scaledRed.Zip(scaledWhite, (r, w) => (byte) (r + w)).ToArray();
            var mColor = Color.FromRgb(mColorRGB[0], mColorRGB[1], mColorRGB[2]);
            return mColor;
        }
    }
}