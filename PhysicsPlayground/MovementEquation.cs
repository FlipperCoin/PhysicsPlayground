using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsPlayground
{
    class MovementEquation
    {
        public float Ax0 { get; set; }
        public float Ay0 { get; set; }
        public float Vx0 { get; set; }
        public float Vy0 { get; set; }
        public double X0 { get; set; }
        public double Y0 { get; set; }

        public (double x, double y) GetLocationInTime(double t)
        {
            double x = X0 + 0.5 * Ax0 * Math.Pow(t, 2) + Vx0 * t;
            double y = Y0 + 0.5 * Ay0 * Math.Pow(t, 2) + Vy0 * t;

            return (x, y);
        }
    }
}
