using CenterSpace.NMath.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsPlayground.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var ax = new Polynomial(new DoubleVector("9.8"));
            var vx = (ax.AntiDerivative() + new Polynomial(new DoubleVector("-10")));
            var x = (vx.AntiDerivative() + new Polynomial(new DoubleVector("-100")));
            Console.WriteLine("f(x) = " + x.ToString());
            Console.WriteLine("f(2) = " + x.Evaluate(15));
        }
    }
}
