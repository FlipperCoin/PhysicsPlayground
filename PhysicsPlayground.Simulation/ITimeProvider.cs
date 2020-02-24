using System;

namespace PhysicsPlayground.Simulation
{
    public interface ITimeProvider
    {
        TimeSpan Time { get; }

        public double Speed { get; set; }
    }
}