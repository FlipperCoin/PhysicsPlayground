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
                Polynomial 
                    xPol1 = new Polynomial(), 
                    xPol2 = new Polynomial(), 
                    yPol1 = new Polynomial(), 
                    yPol2 = new Polynomial();

                for (var i = 0; i < objects.Count; i++)
                {
                    (_, ((_, xPol1), (_, yPol1))) = objects[i];
                    for (var j = i + 1; j < objects.Count; j++)
                    {
                        (_, ((_, xPol2), (_, yPol2))) = objects[j];

                        // TODO: Check what to do for the root of a continuous f(x) = 0 (yPol1 = yPol2 or xPol1 = xPol2)

                        var xRoots = xPol1.Roots(xPol2, t, t2).OrderBy(root => root);
                        var yRoots = yPol1.Roots(yPol2, t, t2).OrderBy(root => root);

                        if (xRoots.Any() && yRoots.Any() && xRoots.First().AlmostEqual(yRoots.First(), 5e-6))
                        {
                            var pairMinRoot = xRoots.First();

                            if (pairMinRoot.CompareTo(minRoot) < 0)
                            {
                                minRoot = pairMinRoot;
                                obj1Index = i;
                                obj2Index = j;
                            }
                        }
                    }
                }

                if (!obj1Index.HasValue) break;

                var (massObj1, ((xEquations1, _), (yEquations1, _))) = objects[obj1Index.Value];
                var (massObj2, ((xEquations2, _), (yEquations2, _))) = objects[obj2Index.Value];

                var nextInterval = (Endpoints.Closed(minRoot), Endpoints.Unbounded);

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

                t = minRoot;
            }

            return new Simulation(objects.Select(obj =>
                new MovementEquation(obj.Item2.Item1.xEquations, obj.Item2.Item2.yEquations)).ToList());
        }
    }
}
