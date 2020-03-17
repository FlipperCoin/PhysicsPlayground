using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PhysicsPlayground.Simulation.Simulator.Implementations;

namespace PhysicsPlayground.Display.DisplayAdapters
{
    internal class RocketDisplayAdapter : IDisplayAdapter<RocketSimulationMoment>
    {
        private readonly IScreenParametersProvider _screenParams;

        public RocketDisplayAdapter(IScreenParametersProvider screenParams)
        {
            _screenParams = screenParams;
        }

        public IEnumerable<Shape> GetDrawables(RocketSimulationMoment adaptable)
        {
            var rocket = new Rectangle();
            rocket.Width = 1 * _screenParams.PixelsPerMeter;
            rocket.Height = 10 * _screenParams.PixelsPerMeter;
            rocket.Fill = new SolidColorBrush(Colors.Black);
            
            Canvas.SetTop(rocket, (_screenParams.YCenter - 10 * _screenParams.PixelsPerMeter) - adaptable.Y.D * _screenParams.PixelsPerMeter);
            Canvas.SetLeft(rocket, _screenParams.XCenter - 0.5 * _screenParams.PixelsPerMeter);

            return Enumerable.Repeat(rocket, 1);
        }
    }
}