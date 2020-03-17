using static System.Double;

namespace PhysicsPlayground.DataStructures
{
    public static class Endpoints
    {
        public static (double, EndpointType) Unbounded => (NaN, EndpointType.Unbounded);
        public static (double, EndpointType) Open(double val) => (val, EndpointType.Open);
        public static (double, EndpointType) Closed(double val) => (val, EndpointType.Closed);
    }
}
