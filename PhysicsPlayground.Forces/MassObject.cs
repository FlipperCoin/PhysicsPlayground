using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PhysicsPlayground.Forces
{
    public class MassObject
    {
        public Guid Guid { get; internal set; }
        public float Mass { get; set; }
        public IList<Force> Forces { get; set; }
        public MovementEquationConstants InitValues { get; set; }

        public MassObject(int mass, IList<Force> forces, MovementEquationConstants initValues)
        {
            Guid = Guid.NewGuid();

            Mass = mass;
            Forces = forces;
            InitValues = initValues;
        }

        public (double x, double y) GetLocationInTime(double t)
        {
            var aVector = Forces
                .Select(force => Vector2.Divide(force.Vector, Mass))
                .Aggregate((force1Vector, force2Vector) => Vector2.Add(force1Vector, force2Vector));
            (float ax, float ay) = (aVector.X, aVector.Y);

            double x = InitValues.X0 + 0.5 * (InitValues.Ax0 + aVector.X) * Math.Pow(t, 2) + InitValues.Vx0 * t;
            double y = InitValues.Y0 + 0.5 * (InitValues.Ay0 + aVector.Y) * Math.Pow(t, 2) + InitValues.Vy0 * t;

            return (x, y);
        }
    }
}
