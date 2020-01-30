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
        private IEnumerable<(MassObject, MovementParameters)> _objectsAndMovementParameters;

        public ConservationOfMomentumEngine(GridParams grid, IEnumerable<(MassObject, InitialMovementParameters)> objectsAndTheirInitialMovementParameters)
        {
            _grid = grid;
            _objectsAndMovementParameters = objectsAndTheirInitialMovementParameters
                .Select(objAndInitMovementParams =>
                {
                    var (obj, initMovementParams) = objAndInitMovementParams;
                    return (obj, new MovementParameters()
                    {
                        Vx = initMovementParams.Vx0,
                        Vy = initMovementParams.Vy0,
                        X = initMovementParams.X0,
                        Y = initMovementParams.Y0
                    });
                }).ToList();
        }
        public IEnumerable<(double, double)> GetCoordinates(double t)
        {
            var coordinates = new List<(double, double)>();
            var updatedMovementParams = _objectsAndMovementParameters.Select((objAndMovementParams) =>
            {
                var (obj, movementParams) = objAndMovementParams;
                var (x, y) = GetLocationInTime(t, movementParams);
                // I wanted to update x, y, vx, vy but realized t should be changed too
                // Different approach is needed, managing real movement functions with t in an offset might be hard,
                // need to check
                coordinates.Add((x, y));
                return (obj, movementParams);
            }).ToList();
            _objectsAndMovementParameters = updatedMovementParams;
            return coordinates;
        }

        private (double x, double y) GetLocationInTime(double t, MovementParameters movementParams)
        {
            double x = movementParams.X + movementParams.Vx * t;
            double y = movementParams.Y + movementParams.Vy * t;

            return (x, y);
        }
    }
}
