using System;
using System.Collections.Generic;
using System.Text;

namespace PhysicsPlayground.DataStructures
{
    public class Interval
    {
        public (double, EndpointType) LowerEndpoint { get; set; }
        public (double, EndpointType) UpperEndpoint { get; set; }
    }
}
