using System;

namespace PhysicsPlayground.Simulation
{
    class BoundedEndpointBase : IntervalEndpointBase
    {
        public double Value { get; }

        public BoundedEndpointType Type { get; }

        public BoundedEndpointBase(double value, BoundedEndpointType type)
        {
            Value = value;
            Type = type;
        }

        public override bool InRangeBelow(double t)
        {
            return Type switch
            {
                BoundedEndpointType.Open => t < this,
                BoundedEndpointType.Closed => t <= this,
                _ => throw new Exception()
            };
        }

        public override bool InRangeAbove(double t)
        {
            return Type switch
            {
                BoundedEndpointType.Open => t > this,
                BoundedEndpointType.Closed => t >= this,
                _ => throw new Exception()
            };
        }

        public override bool Smaller(double t) => t < Value;

        public override bool Larger(double t) => t > Value;

        public override bool Equals(double t) => t == Value;
    }
}