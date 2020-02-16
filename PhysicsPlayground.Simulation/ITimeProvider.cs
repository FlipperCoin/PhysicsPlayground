using System;

namespace PhysicsPlayground.Simulation
{
    public interface ITimeProvider
    {
        TimeSpan Time { get; }
    }
}