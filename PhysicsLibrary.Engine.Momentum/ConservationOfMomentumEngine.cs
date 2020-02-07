using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhysicsPlayground.Engine;

namespace PhysicsLibrary.Engine.Momentum
{
    public class ConservationOfMomentumEngine : IEngine
    {
        private readonly GridParams _grid;
        private IEnumerable<(MassObject, MovementEquation)> _objectsAndMovementParameters;

        public ConservationOfMomentumEngine(GridParams grid, IEnumerable<(MassObject, MovementParameters2)> objectsAndTheirInitialMovementParameters)
        {
            _grid = grid;
            _objectsAndMovementParameters = objectsAndTheirInitialMovementParameters
                .Select(objAndMovementParams =>
                {
                    var (obj, movementParams2) = objAndMovementParams;
                    return (obj, new MovementEquation(movementParams2.X, movementParams2.Y));
                }).ToList();
        }

        public IEnumerable<(double, double)> GetCoordinates(double t)
        {
            var coordinates = new List<(double, double)>();
            var updatedMovementParams = _objectsAndMovementParameters.Select((objAndMovementEquation) =>
            {
                var (obj, movementEquation) = objAndMovementEquation;
                var (x, y) = movementEquation.GetLocationInTime(t);
                var (vx, vy) = movementEquation.GetSpeedInTime(t);
                var (ax, ay) = movementEquation.GetAccelerationInTime(t);
                if ((x <= 0 && vx < 0) || (x >= _grid.X && vx > 0))
                {
                    movementEquation.UpdateXEquations(
                        new Interval()
                        {
                            Minimum = new IntervalEndpoint() { Type = EndpointType.Closed, Value = t },
                            Maximum = IntervalEndpoint.Unbounded
                        }, 
                        new InitialMovementParameters { D0 = x, V0 = -vx, A0 = ax, T0=t}
                    );
                }
                if ((y <= 0 && vy < 0) || (y >= _grid.Y && vy > 0))
                {
                    movementEquation.UpdateYEquations(
                        new Interval()
                        {
                            Minimum = new IntervalEndpoint() { Type = EndpointType.Closed, Value = t },
                            Maximum = IntervalEndpoint.Unbounded
                        },
                        new InitialMovementParameters { D0 = y, V0 = -vy, A0 = ay, T0 = t }
                    );
                }

                coordinates.Add((x, y));
                return (obj, movementEquation);
            }).ToList();
            _objectsAndMovementParameters = updatedMovementParams;
            return coordinates;
        }
    }
}
