using System.Collections.Generic;
using System.Linq;

namespace PhysicsPlayground.Simulation.Implementations
{
    public class PlaneMovementSimulation<T> : ISimulation<IEnumerable<(T, (double, double))>>
    {
        private readonly IEnumerable<(T obj, MovementEquation movement)> _movementEquations;

        public PlaneMovementSimulation(IEnumerable<(T, MovementEquation)> movementEquations)
        {
            _movementEquations = movementEquations;
        }

        public IEnumerable<(T, (double, double))> GetMomentInTime(double t) => 
            _movementEquations.Select(objAndMovement =>
                (objAndMovement.obj, objAndMovement.movement.GetLocationInTime(t)));
    }
}