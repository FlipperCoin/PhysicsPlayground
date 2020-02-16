using System.Collections.Generic;

namespace PhysicsPlayground.Simulation
{
    public class SimulationRunner : IObjectsStateProvider
    {
        private readonly ISimulation _simulation;
        private readonly ITimeProvider _timeProvider;

        public SimulationRunner(ISimulation simulation, ITimeProvider timeProvider)
        {
            _simulation = simulation;
            _timeProvider = timeProvider;
        }

        public IEnumerable<(double x, double y)> GetCoordinates()
        {
            return _simulation.GetCoordinates(_timeProvider.Time.TotalSeconds);
        }
    }
}