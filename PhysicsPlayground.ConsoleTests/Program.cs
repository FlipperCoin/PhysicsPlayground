using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using PhysicsPlayground.Math;
using Polynomial = PhysicsPlayground.Math.Polynomial;

namespace PhysicsPlayground.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Polynomial p = new Polynomial(36,0,-7,1);
            Complex[] complexRoots = FindRoots.Polynomial(p.Coefficients);
            var roots = complexRoots.Where(c => c.IsReal()).Distinct().Select(c => c.Real).ToList();
            roots.ForEach(Console.WriteLine);
        }
    }
}
