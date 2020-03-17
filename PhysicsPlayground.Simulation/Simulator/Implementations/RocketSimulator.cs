using System;
using PhysicsPlayground.DataStructures;
using PhysicsPlayground.Simulation.Implementations;

namespace PhysicsPlayground.Simulation.Simulator.Implementations
{
    public class RocketSimulator : SyncSimulator<RocketSimulationMoment>
    {
        private readonly double _baseMass;
        private readonly double _propellantMass;
        private readonly double _massLossRate;
        private readonly double _effectiveExhaustVelocity;
        private readonly double _v0;

        public RocketSimulator(double baseMass, double propellantMass, double massLossRate, double effectiveExhaustVelocity, double v0)
        {
            _baseMass = baseMass;
            _propellantMass = propellantMass;
            _massLossRate = massLossRate;
            _effectiveExhaustVelocity = effectiveExhaustVelocity;
            _v0 = v0;
        }

        public override ISimulation<RocketSimulationMoment> GenerateSimulation(double t1, double t2)
        {
            double burnTime = t1 + _propellantMass / _massLossRate;

            var xa = new IntervalIndexer<Func<double, double>>();
            xa.AddInterval((Endpoints.Closed(t1), Endpoints.Closed(burnTime)),
                (t) => RocketAEquation(t, t1));
            xa.AddInterval((Endpoints.Open(burnTime), Endpoints.Unbounded),
                (t) => 0);

            var xv = new IntervalIndexer<Func<double, double>>();
            xv.AddInterval((Endpoints.Closed(t1), Endpoints.Closed(burnTime)),
                (t) => RocketVEquation(t, t1));
            xv.AddInterval((Endpoints.Open(burnTime), Endpoints.Unbounded), 
                (t) => RocketVEquation(burnTime, t1));

            var x = new IntervalIndexer<Func<double, double>>();
            x.AddInterval((Endpoints.Closed(t1), Endpoints.Closed(burnTime)),
                (t) => RocketXEquation(t, t1));
            x.AddInterval((Endpoints.Open(burnTime), Endpoints.Unbounded),
                (t) => RocketXEquation(burnTime, t1) + RocketVEquation(burnTime, t1) * (t - burnTime));

            return new RocketSimulation(_baseMass, _propellantMass, _massLossRate, burnTime, xa, xv, x);
        }

        private double RocketAEquation(double t, double t0)
        {
            return ((-_massLossRate * _effectiveExhaustVelocity) / (_baseMass + _propellantMass)) /
                   ((1 - _massLossRate * (t - t0)) / (_baseMass + _propellantMass));
        }

        private double RocketVEquation(double t, double t0)
        {
            return (_v0 + _effectiveExhaustVelocity * System.Math.Log(
                        (_baseMass + _propellantMass) /
                        (_baseMass + _propellantMass - (_massLossRate * (t - t0)))
                    ));
        }

        private double RocketXEquation(double t, double t0)
        {
            var m0 = _baseMass + _propellantMass;
            var alpha = _massLossRate;
            return _effectiveExhaustVelocity * (((t - t0) - (m0 / alpha)) * System.Math.Log(m0 / (m0 - alpha * (t - t0))) + (t - t0)) +_v0 * (t - t0);
        }
    }

    internal class RocketSimulation : ISimulation<RocketSimulationMoment>
    {
        private readonly double _baseMass;
        private readonly double _fuelMass;
        private readonly double _changeOfMassRate;
        private readonly double _burnTime;
        private readonly IntervalIndexer<Func<double, double>> _ya;
        private readonly IntervalIndexer<Func<double, double>> _yv;
        private readonly IntervalIndexer<Func<double, double>> _y;

        public RocketSimulation(double baseMass,double fuelMass, double changeOfMassRate, double burnTime,
            IntervalIndexer<Func<double, double>> ya, IntervalIndexer<Func<double,double>> yv, IntervalIndexer<Func<double, double>> y)
        {
            _baseMass = baseMass;
            _fuelMass = fuelMass;
            _changeOfMassRate = changeOfMassRate;
            _burnTime = burnTime;
            _ya = ya;
            _yv = yv;
            _y = y;
        }

        public RocketSimulationMoment GetMomentInTime(double t)
        {
            return new RocketSimulationMoment
            {
                Y = new AxisMovementParameters
                {
                    A = _ya[t].Value.Invoke(t),
                    V = _yv[t].Value.Invoke(t),
                    D = _y[t].Value.Invoke(t),
                }
            };
        }
    }

    public class RocketSimulationMoment
    {
        public AxisMovementParameters Y { get; set; }
    }
}
