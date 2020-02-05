using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace PhysicsLibrary.Engine.Momentum
{

    class Interval
    {
        public IntervalEndpoint Minimum { get; set; }
        public IntervalEndpoint Maximum { get; set; }

        public void Deconstruct(out IntervalEndpoint minimum, out IntervalEndpoint maximum)
        {
            minimum = Minimum;
            maximum = Maximum;
        }
    }

    public class IntervalEndpoint
    {
        public EndpointType Type { get; set; }
        public double Value { get; set; }

        public static IntervalEndpoint Unbounded
        {
            get 
            {
                var (val, type) = Endpoints.Unbounded;
                return new IntervalEndpoint() { Type = type, Value = val };
            }
        }

        public void Deconstruct(out double value, out EndpointType type)
        {
            value = Value;
            type = Type;
        }

    }

    class GenericInterval
    {
        public IntervalEndpointBase Minimum { get; set; }
        public IntervalEndpointBase Maximum { get; set; }

        public bool Contains(double scalar)
        {
            return Minimum.InRangeAbove(scalar) && Maximum.InRangeBelow(scalar);
        }
    }

    internal enum IntervalType
    {
        Unbounded,
        Closed,
        Open,
        LeftOpen,
        RightOpen
    }
}
