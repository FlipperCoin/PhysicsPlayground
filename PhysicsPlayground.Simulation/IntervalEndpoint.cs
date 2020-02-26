namespace PhysicsPlayground.Simulation
{
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
}