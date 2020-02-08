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
        public double A0 { get; set; }
        public double V0 { get; set; }
        public double D0 { get; set; }
        public double T0 { get; set; }
    }
}
