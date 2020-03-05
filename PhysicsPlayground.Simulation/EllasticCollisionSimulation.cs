using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using PhysicsPlayground.Simulation.Simulators;

namespace PhysicsPlayground.Simulation
{
    public class EllasticCollisionSimulation : ISimulation<ElasticCollisionMoment>
    {
        private readonly Box _box;
        private IEnumerable<(MassEllipse, MovementEquation)> _movementEquations;

        public EllasticCollisionSimulation(Box box, IEnumerable<(MassEllipse, MovementEquation)> movementEquations)
        {
            _box = box;
            _movementEquations = movementEquations;
        }

        public ElasticCollisionMoment GetMomentInTime(double t)
        {
            var balls = _movementEquations.Select(eq =>
            {
                var (x, y) = eq.Item2.GetEquationsInTime(t);
                return (eq.Item1, new MassEllipseParameters()
                {
                    EllipseMovementParameters = new EllipseMovementParameters()
                    {
                        X = new AxisMovementParameters()
                        {
                            D = x.Evaluate(t),
                            V = x.Derivative().Evaluate(t),
                            A = x.Derivative().Derivative().Evaluate(t)
                        },
                        Y = new AxisMovementParameters()
                        {

                            D = y.Evaluate(t),
                            V = y.Derivative().Evaluate(t),
                            A = y.Derivative().Derivative().Evaluate(t)
                        }
                    }
                });
            });

            var totalXMomentum = balls
                .Select(ball => ball.Item1.Mass * ball.Item2.EllipseMovementParameters.X.V)
                .Aggregate((v1,v2) => v1 + v2);
            var totalYMomentum = balls
                .Select(ball => ball.Item1.Mass * ball.Item2.EllipseMovementParameters.Y.V)
                .Aggregate((v1,v2) => v1 + v2);

            return new ElasticCollisionMoment()
            {
                Balls = balls,
                EllasticCollisionMetadata = new EllasticCollisionMetadata
                {
                    Box = _box,
                    TotalXMomentum = totalXMomentum,
                    TotalYMomentum = totalYMomentum
                }
            };
        }
    }

    public class ElasticCollisionMoment
    {
        public EllasticCollisionMetadata EllasticCollisionMetadata { get; set; }
        public IEnumerable<(MassEllipse, MassEllipseParameters)> Balls { get; set; }
    }

    public class MassEllipseParameters
    {
        public EllipseMovementParameters EllipseMovementParameters { get; set; }
    }

    public class EllipseMovementParameters
    {
        public AxisMovementParameters X { get; set; }
        public AxisMovementParameters Y { get; set; }
    }

    public class AxisMovementParameters
    {
        public double D { get; set; }
        public double V { get; set; }
        public double A { get; set; }
    }

    public class EllasticCollisionMetadata
    {
        public Box Box { get; set; }
        public double TotalXMomentum { get; set; }
        public double TotalYMomentum { get; set; }
    }
}
