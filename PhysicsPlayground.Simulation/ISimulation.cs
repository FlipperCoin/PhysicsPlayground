using System.Collections.Generic;

namespace PhysicsPlayground.Simulation
{
    public interface ISimulation
    {
        IEnumerable<(double, double)> GetCoordinates(double t);
    }
}