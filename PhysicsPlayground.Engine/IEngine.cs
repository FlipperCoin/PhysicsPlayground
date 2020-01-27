using System;
using System.Collections.Generic;

namespace PhysicsPlayground.Engine
{
    public interface IEngine
    {
        IEnumerable<(double, double)> GetCoordinates(double t);
    }
}
