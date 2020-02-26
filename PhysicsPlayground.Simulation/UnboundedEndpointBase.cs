namespace PhysicsPlayground.Simulation
{
    class UnboundedEndpointBase : IntervalEndpointBase
    {
        public bool Sign { get; }

        public UnboundedEndpointBase(bool sign)
        {
            Sign = sign;
        }

        public override bool InRangeBelow(double t) => t < this;

        public override bool InRangeAbove(double t) => t > this;

        public override bool Smaller(double t)
        {
            return !Sign;
        }

        public override bool Larger(double t)
        {
            return Sign;
        }

        public override bool Equals(double t) => false;
    }
}