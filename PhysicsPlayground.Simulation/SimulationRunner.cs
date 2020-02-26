using System.Collections.Generic;

namespace PhysicsPlayground.Simulation
{
    public class SimulationRunner<T> : IObjectProvider<T>
    {
        private readonly ISimulation<T> _simulation;
        private readonly ITimeProvider _timeProvider;

        public SimulationRunner(ISimulation<T> simulation, ITimeProvider timeProvider)
        {
            _simulation = simulation;
            _timeProvider = timeProvider;
        }

        public T GetMoment()
        {
            return _simulation.GetMomentInTime(_timeProvider.Time.TotalSeconds);
        }

        public T GetObject()
        {
            return GetMoment();
        }
    }

    public class SimulationRunner : SimulationRunner<IEnumerable<(double, double)>>
    {
        public SimulationRunner(ISimulation<IEnumerable<(double, double)>> simulation, ITimeProvider timeProvider) : base(simulation, timeProvider)
        {
        }
    }
}