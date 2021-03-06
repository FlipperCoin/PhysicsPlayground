﻿using System.Collections.Generic;
using System.Windows.Shapes;

namespace PhysicsPlayground.Display
{
    internal interface IShapesProvider
    {
        IEnumerable<Shape> Shapes { get; }
    }

    internal interface IMetadataProvider
    {
        string Metadata { get; }
    }
}