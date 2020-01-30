using System.Collections.Generic;
using System.Numerics;

namespace PhysicsPlayground.Engine.ConstantForces
{
    public class ForcesEngineFactory : IEngineFactory
    {
        private readonly GridParams _grid;

        public ForcesEngineFactory(GridParams grid)
        {
            _grid = grid;
        }
        public IEngine GetEngine()
        {
            return new ForcesEngine(
                new List<MassObject>
                {
                    new MassObject(
                    10,
                    new List<Force> { new Force { Vector = new Vector2(0, 98) } },
                    new MovementEquationConstants { X0 = 0, Y0 = 2 * _grid.Y / 3, Ax0 = 0, Ay0 = 0, Vx0 = 10, Vy0 = -10}),
                new MassObject(
                    10,
                    new List<Force> { new Force { Vector = new Vector2(0, 98) } },
                    new MovementEquationConstants { X0 = 0, Y0 = 2 * _grid.Y / 3, Ax0 = 0, Ay0 = 0, Vx0 = 10, Vy0 = -15}),
                new MassObject(
                    10,
                    new List<Force> { new Force { Vector = new Vector2(0, 98) } },
                    new MovementEquationConstants { X0 = 0, Y0 = 2 * _grid.Y / 3, Ax0 = 0, Ay0 = 0, Vx0 = 10, Vy0 = -20}),

                }
            );
        }
    }
}
