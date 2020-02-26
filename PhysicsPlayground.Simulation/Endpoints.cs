using MathNet.Numerics.Integration;
using static System.Double;

namespace PhysicsPlayground.Simulation
{
    public static class Endpoints
    {
        public static (double, EndpointType) Unbounded => (NaN, EndpointType.Unbounded);
        public static (double, EndpointType) Open(double val) => (val, EndpointType.Open);
        public static (double, EndpointType) Closed(double val) => (val, EndpointType.Closed);
    }
}
