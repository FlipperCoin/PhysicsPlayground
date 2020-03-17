using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhysicsPlayground.Simulation.Simulator
{
    public abstract class AsyncSimulator<T> : ISimulator<T>
    {
        public abstract Task<ISimulation<T>> GenerateSimulationAsync(double t1, double t2);

        public ISimulation<T> GenerateSimulation(double t1, double t2) =>
            GenerateSimulationAsync(t1, t2).Result;

    }
    public abstract class AsyncSimulator : AsyncSimulator<IEnumerable<(double, double)>>
    {

    }
}