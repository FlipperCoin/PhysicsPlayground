using PhysicsPlayground.Engine;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PhysicsPlayground.Forces
{
    public class ForcesEngineFactory : IEngineFactory
    {
        public IEngine GetEngine(GridParams grid)
        {
            return new ForcesEngine(
                new List<MassObject>
                {
                    new MassObject(
                    10,
                    new List<Force> { new Force { Vector = new Vector2(0, 98) } },
                    new MovementEquationConstants { X0 = 0, Y0 = 2 * grid.Y / 3, Ax0 = 0, Ay0 = 0, Vx0 = 10, Vy0 = -10}),
                new MassObject(
                    10,
                    new List<Force> { new Force { Vector = new Vector2(0, 98) } },
                    new MovementEquationConstants { X0 = 0, Y0 = 2 * grid.Y / 3, Ax0 = 0, Ay0 = 0, Vx0 = 10, Vy0 = -15}),
                new MassObject(
                    10,
                    new List<Force> { new Force { Vector = new Vector2(0, 98) } },
                    new MovementEquationConstants { X0 = 0, Y0 = 2 * grid.Y / 3, Ax0 = 0, Ay0 = 0, Vx0 = 10, Vy0 = -20}),

                }
            );
        }
    }
}
