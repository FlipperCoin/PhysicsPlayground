using System.Threading.Tasks;

namespace PhysicsPlayground.Simulation.Simulator
{
    public interface ISimulator<T>
    {
        /// <summary>
        /// Generates a simulation in the provided closed interval
        /// </summary>
        /// <param name="t1">starting point</param>
        /// <param name="t2">ending point</param>
        /// <returns>the generated simulation</returns>
        Task<ISimulation<T>> GenerateSimulationAsync(double t1, double t2);

        /// <summary>
        /// Generates a simulation in the provided closed interval
        /// </summary>
        /// <param name="t1">starting point</param>
        /// <param name="t2">ending point</param>
        /// <returns>the generated simulation</returns>
        ISimulation<T> GenerateSimulation(double t1, double t2);
    }
    public interface ISimulator : ISimulator<(double, double)>
    {
    }
}