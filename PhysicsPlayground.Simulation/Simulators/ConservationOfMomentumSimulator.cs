using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using Polynomial = PhysicsPlayground.Math.Polynomial;

namespace PhysicsPlayground.Simulation.Simulators
{
    public class ConservationOfMomentumSimulator : SyncSimulator
    {
        private readonly Box _box;
        private readonly IEnumerable<(MassObject, MovementParameters2)> _objectsAndMovementParameters;

        public ConservationOfMomentumSimulator(Box box, IEnumerable<(MassObject, MovementParameters2)> objectsAndMovementParameters)
        {
            _box = box;
            _objectsAndMovementParameters = objectsAndMovementParameters;
        }

        public override ISimulation<IEnumerable<(double,double)>> GenerateSimulation(double t1, double t2)
        {
            IEnumerable<MovementEquation> movementEquations = _objectsAndMovementParameters
                .Select(obj =>
                {
                    var (massObject, (x, y)) = obj;

                    var xEquations = BuildAxisEquations(t1, t2, x, _box.X1, _box.X2);
                    var yEquations = BuildAxisEquations(t1, t2, y, _box.Y1, _box.Y2);

                    return new MovementEquation(xEquations, yEquations);
                }).ToList();

            return new Simulation(movementEquations);
        }

        private static IntervalIndexer<Polynomial> BuildAxisEquations(double t1, double t2, InitialMovementParameters axisParams,
            double axisMin, double axisMax)
        {
            var pol = MovementEquation.GetPolynomialMovementEquation(axisParams);
            var axisEquations = new IntervalIndexer<Polynomial>();
            axisEquations.AddInterval((Endpoints.Unbounded, Endpoints.Unbounded), pol);

            var t = t1;
            while (t < t2)
            {
                // Using root > t instead of >= means that putting the object on the edge from the start
                // means that it will go the direction it was set to
                var rootsWithMin = pol.Roots(axisMin).Where(root => root.CompareTo(t,5e-6) == 1 && root <= t2);
                var rootsWithMax = pol.Roots(axisMax).Where(root => root.CompareTo(t, 5e-6) == 1 && root <= t2);

                var roots = rootsWithMin.Concat(rootsWithMax);
                if (!roots.Any())
                {
                    break;
                }

                var minRoot = roots.Min();

                axisParams = new InitialMovementParameters()
                {
                    D0 = pol.Evaluate(minRoot), V0 = -pol.Derivative().Evaluate(minRoot), A0 = pol.Derivative().Derivative().Evaluate(minRoot), T0 = minRoot
                };
                pol = MovementEquation.GetPolynomialMovementEquation(axisParams);
                axisEquations.AddInterval((Endpoints.Closed(minRoot), Endpoints.Open(t2)), pol);
                t = minRoot;
            }

            return axisEquations;
        }
    }
}
