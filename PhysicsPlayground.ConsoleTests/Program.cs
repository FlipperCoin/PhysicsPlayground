using CenterSpace.NMath.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterSpace.NMath.Analysis;

namespace PhysicsPlayground.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var v_ax = new Polynomial(new DoubleVector("9.8"));
            var v_vx = (v_ax.AntiDerivative() + new Polynomial(new DoubleVector("10")));
            var v_x = (v_vx.AntiDerivative() + new Polynomial(new DoubleVector("-100")));

            var u_ax = new Polynomial(new DoubleVector("9.7"));
            var u_vx = (u_ax.AntiDerivative() + new Polynomial(new DoubleVector("10")));
            var u_x = (u_vx.AntiDerivative() + new Polynomial(new DoubleVector("100")));

            var deltaUxVx = v_x - u_x;
            var rootFinder = new NewtonRaphsonRootFinder();

            try
            {
                var meetingPoint = rootFinder.Find(deltaUxVx, deltaUxVx.Derivative(),
                    new Interval(0, 100, Interval.Type.Closed));
                Console.WriteLine(meetingPoint);
            } catch(NMathException e) {
                Console.WriteLine(e.Message);
            }

        }
    }
}
