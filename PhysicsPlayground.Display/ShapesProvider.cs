using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using PhysicsPlayground.Display.DisplayAdapters;
using PhysicsPlayground.Runtime;
using PhysicsPlayground.Simulation;

namespace PhysicsPlayground.Display
{
    class ShapesProvider : IShapesProvider
    {
        private readonly Func<IEnumerable<Shape>> _getShapes;
        public IEnumerable<Shape> Shapes => _getShapes();

        public ShapesProvider(Func<IEnumerable<Shape>> getShapes)
        {
            _getShapes = getShapes;
        }

        public static ShapesProvider CreateInstance<T>(IObjectProvider<T> objectProvider,
            IDisplayAdapter<T> displayAdapter)
        {
            return new ShapesProvider(() => displayAdapter.GetDrawables(objectProvider.GetObject()));
        }
    }
}