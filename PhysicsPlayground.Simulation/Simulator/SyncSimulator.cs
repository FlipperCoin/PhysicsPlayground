using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhysicsPlayground.Simulation.Simulator
{
    public abstract class SyncSimulator<T> : ISimulator<T>
    {
        public Task<ISimulation<T>> GenerateSimulationAsync(double t1, double t2) =>
            Task.Run(() => GenerateSimulation(t1, t2));

        public abstract ISimulation<T> GenerateSimulation(double t1, double t2);
    }

    public abstract class SyncSimulator : SyncSimulator<IEnumerable<(double, double)>>
    {
    }
}