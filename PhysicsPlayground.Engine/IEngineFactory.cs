using System;
using System.Collections.Generic;
using System.Text;

namespace PhysicsPlayground.Engine
{
    public interface IEngineFactory
    {
        IEngine GetEngine(GridParams grid);
    }
}
