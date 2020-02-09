namespace PhysicsPlayground.Simulation
{
    public class MovementParameters2
    {
        public InitialMovementParameters X { get; set; }
        public InitialMovementParameters Y { get; set; }

        public void Deconstruct(out InitialMovementParameters x, out InitialMovementParameters y)
        {
            x = X;
            y = Y;
        }
    }

    public class InitialMovementParameters
    {
        public InitialMovementParameters()
        {
            
        }

        public InitialMovementParameters(double a0, double v0, double d0, double t0)
        {
            A0 = a0;
            V0 = v0;
            D0 = d0;
            T0 = t0;
        }

        public InitialMovementParameters(InitialMovementParameters p) : this(p.A0, p.V0, p.D0, p.T0)
        {
            
        }

        public double A0 { get; set; }
        public double V0 { get; set; }
        public double D0 { get; set; }
        public double T0 { get; set; }
    }
}
