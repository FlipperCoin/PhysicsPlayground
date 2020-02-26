namespace PhysicsPlayground.Simulation
{
    class GenericInterval
    {
        public IntervalEndpointBase Minimum { get; set; }
        public IntervalEndpointBase Maximum { get; set; }

        public bool Contains(double scalar)
        {
            return Minimum.InRangeAbove(scalar) && Maximum.InRangeBelow(scalar);
        }
    }
}