using System.Linq;
using MathNet.Numerics;
using MathNet.Spatial.Euclidean;
using Polynomial = PhysicsPlayground.Math.Polynomial;

namespace PhysicsPlayground.Console.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        private static void EllasticColl()
        {
            var m1 = 4d;
            var m2 = 6d;

            var vx1 = 5d;
            var vx2 = -5d;

            var x1 = new Polynomial(0, 5);
            var x2 = new Polynomial(10, -5);

            var vy1 = 5d;
            var vy2 = 0d;
            var y1 = new Polynomial(0, 5);
            var y2 = new Polynomial(5, 0);

            var r1 = 0.5d;
            var r2 = 0.5d;

            var dx = (x2 - x1);
            var dy = (y2 - y1);

            var meetingPointsPol = (dx ^ 2) + (dy ^ 2) - System.Math.Pow(r1 + r2, 2);
            var roots = FindRoots.Polynomial(meetingPointsPol.Coefficients);
            var t = roots.Where(r => r.IsReal()).Select(r => r.Real).Min();

            Vector2D v1 = new Vector2D(vx1, vy1);
            Vector2D v2 = new Vector2D(vx2, vy2);
            var tangent = new Vector2D(dx.Evaluate(t), dy.Evaluate(t)).Orthogonal.Normalize();

            var v1t = v1.ProjectOn(tangent);
            var u1t = v1t;
            var v1p = v1 - v1t;

            var v2t = v2.ProjectOn(tangent);
            var u2t = v2t;
            var v2p = v2 - v2t;

            var p_p = m1 * v1p.Length + m2 * v2p.Length;
            var e = 0.5 * m1 * System.Math.Pow(v1.Length, 2) + 0.5 * m2 * System.Math.Pow(v2.Length, 2);

            var u1p = new Polynomial(p_p / m1, -m2 / m1);
            var u2pLengths = (0.5 * m1 * ((u1p ^ 2) + System.Math.Pow(u1t.Length, 2)) +
                              0.5 * m2 * ((new Polynomial(0, 1) ^ 2) + System.Math.Pow(u2t.Length, 2))).Roots(e);

            var u1pLength = u1p.Evaluate(u2pLengths.First());
        }
    }
}
