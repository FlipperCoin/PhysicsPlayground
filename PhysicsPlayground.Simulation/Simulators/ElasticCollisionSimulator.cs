using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PhysicsPlayground.Math;
using MathNet.Numerics;
using Polynomial = PhysicsPlayground.Math.Polynomial;

namespace PhysicsPlayground.Simulation.Simulators
{
    public class ElasticCollisionSimulator : SyncSimulator
    {
        private readonly IList<(MassObject, MovementParameters2)> _objectsAndMovementParameters;

        public ElasticCollisionSimulator(IEnumerable<(MassObject, MovementParameters2)> objectsAndMovementParameters)
        {
            _objectsAndMovementParameters = objectsAndMovementParameters.ToList();
        }

        public override ISimulation GenerateSimulation(double t1, double t2)
        {
            var objects = _objectsAndMovementParameters.Select(obj =>
            {
                var (massObj, movementParams2) = obj;

                var xEquations = new IntervalIndexer<Polynomial>();
                var yEquations = new IntervalIndexer<Polynomial>();
                var xPol = MovementEquation.GetPolynomialMovementEquation(movementParams2.X);
                var yPol = MovementEquation.GetPolynomialMovementEquation(movementParams2.Y);
                xEquations.AddInterval((Endpoints.Unbounded, Endpoints.Unbounded), xPol);
                yEquations.AddInterval((Endpoints.Unbounded, Endpoints.Unbounded), yPol);

                return (massObj, ((xEquations, xPol), (yEquations, yPol)));
            }).ToList();

            double t = t1;
            while (t < t2)
            {
                var minRoot = t2;
                int? obj1Index = default, obj2Index = default;

                if (GetNextCollision(t2, objects, t, ref minRoot, ref obj1Index, ref obj2Index)) break;

                UpdateMovementEquations(objects, obj1Index, obj2Index, minRoot);

                t = minRoot;
            }

            return new Simulation(objects.Select(obj =>
                new MovementEquation(obj.Item2.Item1.xEquations, obj.Item2.Item2.yEquations)).ToList());
        }

        private static void UpdateMovementEquations(List<(MassObject massObj, ((IntervalIndexer<Polynomial> xEquations, Polynomial xPol), (IntervalIndexer<Polynomial> yEquations, Polynomial yPol)))> objects, int? obj1Index, int? obj2Index, double minRoot)
        {
            var (massObj1, ((xEquations1, xPol1), (yEquations1, yPol1))) = objects[obj1Index.Value];
            var (massObj2, ((xEquations2, xPol2), (yEquations2, yPol2))) = objects[obj2Index.Value];

            var nextInterval = (Endpoints.Closed(minRoot), Endpoints.Unbounded);

            var xVel1 = xPol1.Derivative().Evaluate(minRoot);
            var yVel1 = yPol1.Derivative().Evaluate(minRoot);
            var v1 = System.Math.Sqrt(System.Math.Pow(xVel1, 2) + System.Math.Pow(yVel1, 2));
            var xVel2 = xPol2.Derivative().Evaluate(minRoot);
            var yVel2 = yPol2.Derivative().Evaluate(minRoot);
            var v2 = System.Math.Sqrt(System.Math.Pow(xVel2, 2) + System.Math.Pow(yVel2, 2));
            var m1 = massObj1.Mass;
            var m2 = massObj2.Mass;

            var pX = m1 * xVel1 + m2 * xVel2;
            var pY = m1 * yVel1 + m2 * yVel2;
            var e = 0.5 * m1 * System.Math.Pow(xVel1, 2) + 0.5 * m2 * System.Math.Pow(xVel1, 2);



            var xParams1 = new InitialMovementParameters()
            {
                D0 = xPol1.Evaluate(minRoot),
                V0 = -xPol1.Derivative().Evaluate(minRoot),
                A0 = xPol1.Derivative().Derivative().Evaluate(minRoot),
                T0 = minRoot
            };

            var newXPol1 = MovementEquation.GetPolynomialMovementEquation(xParams1);
            xEquations1.AddInterval(nextInterval, newXPol1);
            xPol1 = newXPol1;

            var yParams1 = new InitialMovementParameters()
            {
                D0 = yPol1.Evaluate(minRoot),
                V0 = -yPol1.Derivative().Evaluate(minRoot),
                A0 = yPol1.Derivative().Derivative().Evaluate(minRoot),
                T0 = minRoot
            };

            var newYPol1 = MovementEquation.GetPolynomialMovementEquation(yParams1);
            yEquations1.AddInterval(nextInterval, newYPol1);
            yPol1 = newYPol1;

            var xParams2 = new InitialMovementParameters()
            {
                D0 = xPol2.Evaluate(minRoot),
                V0 = -xPol2.Derivative().Evaluate(minRoot),
                A0 = xPol2.Derivative().Derivative().Evaluate(minRoot),
                T0 = minRoot
            };

            var newXPol2 = MovementEquation.GetPolynomialMovementEquation(xParams2);
            xEquations2.AddInterval(nextInterval, newXPol2);
            xPol2 = newXPol2;

            var yParams2 = new InitialMovementParameters()
            {
                D0 = yPol2.Evaluate(minRoot),
                V0 = -yPol2.Derivative().Evaluate(minRoot),
                A0 = yPol2.Derivative().Derivative().Evaluate(minRoot),
                T0 = minRoot
            };

            var newYPol2 = MovementEquation.GetPolynomialMovementEquation(yParams2);
            yEquations2.AddInterval(nextInterval, newYPol2);
            yPol2 = newYPol2;

            objects[obj1Index.Value] = (massObj1, ((xEquations1, xPol1), (yEquations1, yPol1)));
            objects[obj2Index.Value] = (massObj2, ((xEquations2, xPol2), (yEquations2, yPol2)));
        }

        private static bool GetNextCollision(double t2, List<(MassObject massObj, ((IntervalIndexer<Polynomial> xEquations, Polynomial xPol), (IntervalIndexer<Polynomial> yEquations, Polynomial yPol)))> objects, double t, ref double minRoot, ref int? obj1Index,
            ref int? obj2Index)
        {
            for (var i = 0; i < objects.Count; i++)
            {
                var (_, ((_, tmpXPol1), (_, tmpYPol1))) = objects[i];
                for (var j = i + 1; j < objects.Count; j++)
                {
                    var (_, ((_, tmpXPol2), (_, tmpYPol2))) = objects[j];

                    double? pairMinRoot = default;

                    if (tmpXPol1 != tmpXPol2)
                    {
                        var xRoots = tmpXPol1.Roots(tmpXPol2, t, t2);
                        if (!xRoots.Any()) continue;

                        if (tmpYPol1 != tmpYPol2)
                        {
                            var yRoots = tmpYPol1.Roots(tmpYPol2, t, t2);
                            if (!yRoots.Any()) continue;

                            var colPoints = xRoots
                                .Where(xr => yRoots.Any(yr => yr.AlmostEqual(yr, 5e-6)));

                            if (colPoints.Any())
                            {
                                pairMinRoot = colPoints.Min();
                            }
                        }
                        else
                        {
                            pairMinRoot = xRoots.Min();
                        }
                    }
                    else if (tmpYPol1 != tmpYPol2)
                    {
                        var yRoots = tmpYPol1.Roots(tmpYPol2, t, t2);
                        if (!yRoots.Any()) continue;

                        pairMinRoot = yRoots.Min();
                    }
                    else
                    {
                        pairMinRoot = t;
                    }

                    if (!pairMinRoot.HasValue) continue;

                    if (pairMinRoot.Value.CompareTo(minRoot) < 0)
                    {
                        minRoot = pairMinRoot.Value;
                        obj1Index = i;
                        obj2Index = j;
                    }
                }
            }

            if (!obj1Index.HasValue) return true;
            return false;
        }
    }
}
