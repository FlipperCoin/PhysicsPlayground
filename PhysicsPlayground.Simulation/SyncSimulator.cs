using System.Threading.Tasks;

namespace PhysicsPlayground.Simulation
{
    public abstract class SyncSimulator : ISimulator
    {
        public Task<ISimulation> GenerateSimulationAsync(double t1, double t2) =>
            Task.Run(() => GenerateSimulation(t1, t2));

        public abstract ISimulation GenerateSimulation(double t1, double t2);
    }
}