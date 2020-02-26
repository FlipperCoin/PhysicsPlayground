using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PhysicsPlayground.Math;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Serilog;
using Polynomial = PhysicsPlayground.Math.Polynomial;

namespace PhysicsPlayground.Simulation.Simulators
{
    public class ElasticCollisionSimulator : SyncSimulator
    {
        private readonly IList<(MassObject, MovementParameters2)> _objectsAndMovementParameters;
        private readonly ILogger _logger = Log.Logger.ForContext<ElasticCollisionSimulator>();

        public ElasticCollisionSimulator(IEnumerable<(MassObject, MovementParameters2)> objectsAndMovementParameters)
        {
            _objectsAndMovementParameters = objectsAndMovementParameters.ToList();
        }

        public override ISimulation<IEnumerable<(double, double)>> GenerateSimulation(double t1, double t2)
        {
            _logger.Information(
                "Generating elastic collision simulation starting {t1} ending {t2}", 
                t1, 
                t2
                );
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

                if (!GetNextCollision(t2, objects, t, ref minRoot, ref obj1Index, ref obj2Index)) break;

                _logger.Debug("Collision on {t} between {obj1Index} and {obj2Index}",minRoot,obj1Index,obj2Index);

                UpdateMovementEquations(objects, obj1Index, obj2Index, minRoot);

                t = minRoot;
            }

            _logger.Information("Finished elastic collision simulation");

            return new Simulation(objects.Select(obj =>
                new MovementEquation(obj.Item2.Item1.xEquations, obj.Item2.Item2.yEquations)).ToList());
        }

        private void UpdateMovementEquations(List<(MassObject massObj, ((IntervalIndexer<Polynomial> xEquations, Polynomial xPol), (IntervalIndexer<Polynomial> yEquations, Polynomial yPol)))> objects, int? obj1Index, int? obj2Index, double t)
        {
            var (massObj1, ((xEquations1, xPol1), (yEquations1, yPol1))) = objects[obj1Index.Value];
            var (massObj2, ((xEquations2, xPol2), (yEquations2, yPol2))) = objects[obj2Index.Value];

            var nextInterval = (Endpoints.Closed(t), Endpoints.Unbounded);

            var x1 = xPol1;
            var y1 = yPol1;
            var x2 = xPol2;
            var y2 = yPol2;
            var vx1 = xPol1.Derivative().Evaluate(t);
            var vy1 = yPol1.Derivative().Evaluate(t);
            var v1 = new Vector2D(vx1, vy1);
            var vx2 = xPol2.Derivative().Evaluate(t);
            var vy2 = yPol2.Derivative().Evaluate(t);
            var v2 = new Vector2D(vx2, vy2);
            var m1 = massObj1.Mass;
            var m2 = massObj2.Mass;

            var e = 0.5 * m1 * System.Math.Pow(v1.Length, 2) + 0.5 * m2 * System.Math.Pow(v2.Length, 2);

            var dx = (x2 - x1);
            var dy = (y2 - y1);

            var perpendicular = new Vector2D(dx.Evaluate(t), dy.Evaluate(t)).Normalize();
            var tangent = perpendicular.Orthogonal.Normalize();

            var v1t = v1.ProjectOn(tangent);
            var u1t = v1t;
            var v1p = v1 - v1t;

            var v2t = v2.ProjectOn(tangent);
            var u2t = v2t;
            var v2p = v2 - v2t;

            var p_p_vector = (m1 * v1p + m2 * v2p);
            var p_p = ((p_p_vector.AngleTo(perpendicular).Degrees % 360).CompareTo(0, 5e-6) == 0 ? 1 : -1) * p_p_vector.Length;

            var u1p = new Polynomial(p_p / m1, -m2 / m1); // u1p based on u2p
            var u2pLengths = (0.5 * m1 * ((u1p ^ 2) + System.Math.Pow(u1t.Length, 2)) +
                              0.5 * m2 * ((new Polynomial(0, 1) ^ 2) + System.Math.Pow(u2t.Length, 2))).Roots(e);

            var v2p_dir = (v2p.AngleTo(perpendicular).Degrees % 360 == 0 ? 1 : -1);
            var u2pLength = u2pLengths.First(len =>
                len.CompareTo( v2p_dir * v2p.Length, 5e-6) != 0);
            var u1pLength = u1p.Evaluate(u2pLength);

            var u1 = perpendicular * u1pLength + u1t;
            var u2 = perpendicular * u2pLength + u2t;

            var newX1 = MovementEquation.GetPolynomialMovementEquation(
                    x1.Derivative().Derivative().Evaluate(t),
                    u1.X,
                    x1.Evaluate(t),
                    t
                );
            var newY1 = MovementEquation.GetPolynomialMovementEquation(
                    y1.Derivative().Derivative().Evaluate(t),
                    u1.Y,
                    y1.Evaluate(t),
                    t
                );
            var newX2 = MovementEquation.GetPolynomialMovementEquation(
                    x2.Derivative().Derivative().Evaluate(t),
                    u2.X,
                    x2.Evaluate(t),
                    t
                );
            var newY2 = MovementEquation.GetPolynomialMovementEquation(
                    y2.Derivative().Derivative().Evaluate(t),
                    u2.Y,
                    y2.Evaluate(t),
                    t
                );

            xEquations1.AddInterval(nextInterval, newX1);
            yEquations1.AddInterval(nextInterval, newY1);
            xEquations2.AddInterval(nextInterval, newX2);
            yEquations2.AddInterval(nextInterval, newY2);

            _logger.Debug("new params after collision, x1: {newX1}, y1: {newY1}, x2: {newX2}, y2: {newY2}",
                newX1,
                newY1,
                newX2,
                newY2
            );

            objects[obj1Index.Value] = (massObj1, ((xEquations1, newX1), (yEquations1, newY1)));
            objects[obj2Index.Value] = (massObj2, ((xEquations2, newX2), (yEquations2, newY2)));
        }

        private static bool GetNextCollision(double t2, List<(MassObject massObj, ((IntervalIndexer<Polynomial> xEquations, Polynomial xPol), (IntervalIndexer<Polynomial> yEquations, Polynomial yPol)))> objects, double t, ref double minRoot, ref int? obj1Index,
            ref int? obj2Index)
        {
            // minRoot is a ref param and can't be used in anonymous lambda functions :/
            var tmpMinRoot = t2;
            for (var i = 0; i < objects.Count; i++)
            {
                var (_, ((_, x1), (_, y1))) = objects[i];
                for (var j = i + 1; j < objects.Count; j++)
                {
                    var (_, ((_, x2), (_, y2))) = objects[j];

                    var dx = x2 - x1;
                    var dy = y2 - y1;

                    double r1 = 0.5, r2 = 0.5;
                    var meetingPointsPol = (dx ^ 2) + (dy ^ 2) - System.Math.Pow(r1 + r2, 2);
                    var meetingPoints = FindRoots.Polynomial(meetingPointsPol.Coefficients).Where(r => r.IsReal()).Select(r => r.Real).Where(r => r.CompareTo(t, 5e-6) == 1 && r.CompareTo(tmpMinRoot, 5e-6) < 0);
                    
                    if (meetingPoints.Any())
                    {
                        tmpMinRoot = meetingPoints.Min();
                        obj1Index = i;
                        obj2Index = j;
                    }
                }
            }

            minRoot = tmpMinRoot;
            return obj1Index.HasValue;
        }
    }
}
