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
}