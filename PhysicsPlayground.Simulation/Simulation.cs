using System.Collections.Generic;
using System.Linq;

namespace PhysicsPlayground.Simulation
{
    public class Simulation : ISimulation
    {
        private readonly IEnumerable<MovementEquation> _movementEquations;

        public Simulation(IEnumerable<MovementEquation> movementEquations)
        {
            _movementEquations = movementEquations;
        }

        public IEnumerable<(double, double)> GetMomentInTime(double t) =>
            _movementEquations.Select(equation => equation.GetLocationInTime(t));
    }
}