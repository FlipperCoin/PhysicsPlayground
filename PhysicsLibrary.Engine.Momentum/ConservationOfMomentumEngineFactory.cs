using System;
using System.Collections.Generic;
using System.Text;
using PhysicsPlayground.Engine;

namespace PhysicsLibrary.Engine.Momentum
{
    public class ConservationOfMomentumEngineFactory : IEngineFactory
    {
        private readonly GridParams _grid;

        public ConservationOfMomentumEngineFactory(GridParams grid)
        {
            _grid = grid;
        }

        public IEngine GetEngine()
        {
            return new ConservationOfMomentumEngine(
                _grid,
                new List<(MassObject, InitialMovementParameters)>()
                {
                    (new MassObject(10), new InitialMovementParameters() {Vx0 = 10, Vy0 = 10, X0 = 10, Y0 = 10})
                });
        }
    }
}
