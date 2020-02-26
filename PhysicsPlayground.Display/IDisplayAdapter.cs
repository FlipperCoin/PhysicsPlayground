using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;

namespace PhysicsPlayground.Display
{
    internal interface IDisplayAdapter<T>
    {
        public IEnumerable<Shape> GetDrawables(T adaptable);
    }
}
