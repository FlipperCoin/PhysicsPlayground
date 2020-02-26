namespace PhysicsPlayground.Simulation
{

    public class Interval
    {
        public IntervalEndpoint Minimum { get; set; }
        public IntervalEndpoint Maximum { get; set; }

        public void Deconstruct(out IntervalEndpoint minimum, out IntervalEndpoint maximum)
        {
            minimum = Minimum;
            maximum = Maximum;
        }
    }
}
