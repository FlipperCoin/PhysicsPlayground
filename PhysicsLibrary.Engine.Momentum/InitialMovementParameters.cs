namespace PhysicsLibrary.Engine.Momentum
{
    public class MovementParameters2
    {
        public InitialMovementParameters X { get; set; }
        public InitialMovementParameters Y { get; set; }
    }

    public class InitialMovementParameters
    {
        public double A0 { get; set; }
        public double V0 { get; set; }
        public double D0 { get; set; }
        public double T0 { get; set; }
    }
}
