using System;

namespace PhysicsPlayground.Simulation
{
    public class MassObject
    {
        public Guid Guid { get; internal set; }
        public float Mass { get; set; }

        public MassObject(int mass)
        {
            Guid = Guid.NewGuid();

            Mass = mass;
        }
    }
}
