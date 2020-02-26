using System.Collections.Generic;

namespace PhysicsPlayground.Simulation
{
    public interface ISimulation<T>
    {
        T GetMomentInTime(double t);
    }

    public interface ISimulation : ISimulation<IEnumerable<(double, double)>>
    {
    }
}