using System.Collections.Generic;

namespace PhysicsPlayground.Simulation
{
    public interface IObjectsStateProvider
    {
        IEnumerable<(double x, double y)> GetCoordinates();
    }
}