using System.Threading.Tasks;

namespace PhysicsPlayground.Simulation
{
    public abstract class AsyncSimulator : ISimulator
    {
        public abstract Task<ISimulation> GenerateSimulationAsync(double t1, double t2);

        ISimulation ISimulator.GenerateSimulation(double t1, double t2) =>
            GenerateSimulationAsync(t1, t2).Result;

    }
}