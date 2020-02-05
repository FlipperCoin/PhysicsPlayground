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
                new List<(MassObject, MovementParameters2)>()
                {
                    (new MassObject(10), new MovementParameters2()
                    {
                        X= new InitialMovementParameters()
                        {
                            V0 = 10,
                            D0 = 10
                        },
                        Y= new InitialMovementParameters()
                        {
                            V0 = 10,
                            D0 = 10
                        }
                    })
                });
        }
    }
}
