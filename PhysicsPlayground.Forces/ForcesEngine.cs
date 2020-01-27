using PhysicsPlayground.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhysicsPlayground.Forces
{
    class ForcesEngine : IEngine
    {
        private readonly IEnumerable<MassObject> _objects;

        public ForcesEngine(IEnumerable<MassObject> objects)
        {
            _objects = objects;
        }

        public IEnumerable<(double, double)> GetCoordinates(double t)
        {
            return _objects.Select(obj => obj.GetLocationInTime(t));
        }
    }
}
