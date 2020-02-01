using System;
using System.Collections.Generic;
using System.Text;

namespace PhysicsLibrary.Engine.Momentum
{
    class MovementEquation
    {
        public float Ax0 { get; set; }
        public float Ay0 { get; set; }
        public float Vx0 { get; set; }
        public float Vy0 { get; set; }
        public double X0 { get; set; }
        public double Y0 { get; set; }
        public double Tx0 { get; set; }
        public double Ty0 { get; set; }

        public (double x, double y) GetLocationInTime(double t)
        {
            double x = X0 + 0.5 * Ax0 * Math.Pow((t - Tx0), 2) + Vx0 * (t - Tx0);
            double y = Y0 + 0.5 * Ay0 * Math.Pow((t - Ty0), 2) + Vy0 * (t - Ty0);

            return (x, y);
        }
    }
}
