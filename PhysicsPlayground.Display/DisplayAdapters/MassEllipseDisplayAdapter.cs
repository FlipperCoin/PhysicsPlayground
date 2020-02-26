using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PhysicsPlayground.Simulation.Simulators;

namespace PhysicsPlayground.Display
{
    internal class MassEllipseDisplayAdapter : IDisplayAdapter<IEnumerable<(MassEllipse, (double, double))>>
    {
        private readonly IScreenParametersProvider _screenParams;

        public MassEllipseDisplayAdapter(IScreenParametersProvider screenParams)
        {
            _screenParams = screenParams;
        }

        public IEnumerable<Shape> GetDrawables(IEnumerable<(MassEllipse, (double, double))> adaptable)
        {
            return adaptable.Select(ellipseInPlane =>
            {
                var (massEllipse, (x, y)) = ellipseInPlane;

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
            });
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