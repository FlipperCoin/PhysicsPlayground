using System;

namespace PhysicsPlayground.Runtime
{
    public interface ITimeProvider
    {
        TimeSpan Time { get; }

        public double Speed { get; set; }
    }
}