using System.Collections.Generic;
using System.Windows.Shapes;

namespace PhysicsPlayground.Display.DisplayAdapters
{
    internal interface IDisplayAdapter<T>
    {
        public IEnumerable<Shape> GetDrawables(T adaptable);
    }
}
