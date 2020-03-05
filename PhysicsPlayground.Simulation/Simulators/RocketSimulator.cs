using System;
using System.Collections.Generic;
using System.Text;

namespace PhysicsPlayground.Simulation.Simulators
{
    class RocketSimulator : SyncSimulator<RocketSimulationMoment>
    {
        private readonly double _baseMass;
        private readonly double _fuelMass;
        private readonly double _massLossRate;
        private readonly double _exhaustV;
        private readonly double _v0;

        public RocketSimulator(double baseMass, double fuelMass, double massLossRate, double exhaustV, double v0)
        {
            _baseMass = baseMass;
            _fuelMass = fuelMass;
            _massLossRate = massLossRate;
            _exhaustV = exhaustV;
            _v0 = v0;
        }

        public override ISimulation<RocketSimulationMoment> GenerateSimulation(double t1, double t2)
        {
            double burnTime = t1 + _fuelMass / _massLossRate;

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
            xv.AddInterval((Endpoints.Closed(t1), Endpoints.Closed(burnTime)),
                (t) => RocketXEquation(t, t1));
            xv.AddInterval((Endpoints.Open(burnTime), Endpoints.Unbounded),
                (t) => RocketXEquation(burnTime, t1) + RocketVEquation(burnTime, t1) * (t - burnTime));

            return new RocketSimulation(_baseMass, _fuelMass, _massLossRate, burnTime, xa, xv, x);
        }

        private double RocketAEquation(double t, double t0)
        {
            return ((_massLossRate * _exhaustV) / (_baseMass + _fuelMass)) /
                   ((1 - _massLossRate * (t - t0)) / (_baseMass + _fuelMass));
        }

        private double RocketVEquation(double t, double t0)
        {
            return (_v0 + _exhaustV * System.Math.Log(
                        (_baseMass + _fuelMass) /
                        (_baseMass + _fuelMass - (_massLossRate * (t - t0)))
                    ));
        }

        private double RocketXEquation(double t, double t0)
        {
            var m0 = _baseMass + _fuelMass;
            var alpha = _massLossRate;
            return _exhaustV * (((t - t0) - (m0 / alpha)) * System.Math.Log(m0 / (m0 - alpha * (t - t0))) + (t - t0)) +_v0 * (t - t0);
        }
    }

    internal class RocketSimulation : ISimulation<RocketSimulationMoment>
    {
        private readonly double _baseMass;
        private readonly double _fuelMass;
        private readonly double _changeOfMassRate;
        private readonly double _burnTime;
        private readonly IntervalIndexer<Func<double, double>> _xa;
        private readonly IntervalIndexer<Func<double, double>> _xv;
        private readonly IntervalIndexer<Func<double, double>> _x;

        public RocketSimulation(double baseMass,double fuelMass, double changeOfMassRate, double burnTime,
            IntervalIndexer<Func<double, double>> xa, IntervalIndexer<Func<double,double>> xv, IntervalIndexer<Func<double, double>> x)
        {
            _baseMass = baseMass;
            _fuelMass = fuelMass;
            _changeOfMassRate = changeOfMassRate;
            _burnTime = burnTime;
            _xa = xa;
            _xv = xv;
            _x = x;
        }

        public RocketSimulationMoment GetMomentInTime(double t)
        {
            return new RocketSimulationMoment
            {
                X = new AxisMovementParameters
                {
                    A = _xa[t].Value.Invoke(t),
                    V = _xv[t].Value.Invoke(t),
                    D = _x[t].Value.Invoke(t),
                }
            };
        }
    }

    internal class RocketSimulationMoment
    {
        public AxisMovementParameters X { get; set; }
    }
}
