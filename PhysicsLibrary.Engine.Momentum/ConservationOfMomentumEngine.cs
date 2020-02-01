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

        public ConservationOfMomentumEngine(GridParams grid, IEnumerable<(MassObject, InitialMovementParameters)> objectsAndTheirInitialMovementParameters)
        {
            _grid = grid;
            _objectsAndMovementParameters = objectsAndTheirInitialMovementParameters
                .Select(objAndInitMovementParams =>
                {
                    var (obj, initMovementParams) = objAndInitMovementParams;
                    return (obj, new MovementEquation()
                    {
                        Vx0 = initMovementParams.Vx0,
                        Vy0 = initMovementParams.Vy0,
                        X0 = initMovementParams.X0,
                        Y0 = initMovementParams.Y0
                    });
                }).ToList();
        }
        public IEnumerable<(double, double)> GetCoordinates(double t)
        {
            var coordinates = new List<(double, double)>();
            var updatedMovementParams = _objectsAndMovementParameters.Select((objAndMovementParams) =>
            {
                var (obj, movementEquation) = objAndMovementParams;
                var (x, y) = movementEquation.GetLocationInTime(t);

                if (x <= 0 || x >= _grid.X)
                {
                    movementEquation.Tx0 = t;
                    movementEquation.X0 = x;
                    movementEquation.Vx0 = -movementEquation.Vx0;
                }
                if (y <= 0 || y >= _grid.Y)
                {
                    movementEquation.Ty0 = t;
                    movementEquation.Y0 = y;
                    movementEquation.Vy0 = -movementEquation.Vy0;
                }

                coordinates.Add((x, y));
                return (obj, movementEquation);
            }).ToList();
            _objectsAndMovementParameters = updatedMovementParams;
            return coordinates;
        }
    }
}
