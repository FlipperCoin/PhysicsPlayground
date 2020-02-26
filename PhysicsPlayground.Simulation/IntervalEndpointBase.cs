namespace PhysicsPlayground.Simulation
{
    abstract class IntervalEndpointBase
    {
        public abstract bool InRangeBelow(double t);
        public abstract bool InRangeAbove(double t);
        public abstract bool Smaller(double t);
        public abstract bool Larger(double t);
        public abstract bool Equals(double t);
        public static bool operator <(double t, IntervalEndpointBase e) => e.Larger(t);
        public static bool operator <=(double t, IntervalEndpointBase e) => t < e || e.Equals(t);

        public static bool operator >(double t, IntervalEndpointBase e) => e.Smaller(t);
        public static bool operator >=(double t, IntervalEndpointBase e) => t < e || e.Equals(t);
        public static bool operator <(IntervalEndpointBase e, double t) => t > e;
        public static bool operator <=(IntervalEndpointBase e, double t) => t >= e;

        public static bool operator >(IntervalEndpointBase e, double t) => t < e;
        public static bool operator >=(IntervalEndpointBase e, double t) => t <= e;
        //public static bool operator <(IntervalEndpointBase e1, IntervalEndpointBase e2) => t > e;
        //public static bool operator <=(IntervalEndpointBase e1, IntervalEndpointBase e2) => t >= e;

        //public static bool operator >(IntervalEndpointBase e1, IntervalEndpointBase e2) => t < e;
        //public static bool operator >=(IntervalEndpointBase e1, IntervalEndpointBase e2) => t <= e;
    }
}