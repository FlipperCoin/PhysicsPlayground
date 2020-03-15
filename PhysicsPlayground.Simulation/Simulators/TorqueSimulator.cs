using System;
using System.Collections.Generic;
using System.Text;
using PhysicsPlayground.Math;

namespace PhysicsPlayground.Simulation.Simulators
{
    public class SpecificMassEllipse
    {
        public double Radius { get; set; }
        public Polynomial SpecificMassByR { get; set; }

        public SpecificMassEllipse(Polynomial specificMassByR, double radius)
        {
            SpecificMassByR = specificMassByR;
            Radius = radius;
        }
    }

    public class TorqueSimulator : SyncSimulator<AngularMomentumSimulationMoment>
    {
        private readonly SpecificMassEllipse _disc;
        private readonly double _angularVel0;
        private readonly double _force;

        public TorqueSimulator(SpecificMassEllipse disc, double angularVel0, double force)
        {
            _disc = disc;
            _angularVel0 = angularVel0;
            _force = force;
        }

        public override ISimulation<AngularMomentumSimulationMoment> GenerateSimulation(double t1, double t2)
        {
            var (massByR , radius) = (_disc.SpecificMassByR, _disc.Radius);

            var momentOfInertiaByR = massByR * new Polynomial(0, 0, 1);
            var momentOfInertia = momentOfInertiaByR.DefiniteIntegral(radius);

            var angularAcceleration = (_force * radius) / momentOfInertia;

            var angleOverTime = (new Polynomial(angularAcceleration)).AntiDerivative(_angularVel0).AntiDerivative();
            var anglet = new IntervalIndexer<Polynomial>();
            anglet.AddInterval((Endpoints.Closed(t1),Endpoints.Closed(t2)), angleOverTime);

            var defaultIndexer = new IntervalIndexer<Polynomial>(new Polynomial());
            return new AngularMomentumSimulation(_disc, defaultIndexer, anglet, defaultIndexer, defaultIndexer);
        }
    }

    public class AngularMomentumSimulation : ISimulation<AngularMomentumSimulationMoment>
    {
        private readonly SpecificMassEllipse _disk;
        private readonly IntervalIndexer<Polynomial> _omegat;
        private readonly IntervalIndexer<Polynomial> _anglet;
        private readonly IntervalIndexer<Polynomial> _vt;
        private readonly IntervalIndexer<Polynomial> _xt;

        public AngularMomentumSimulation(SpecificMassEllipse disk, IntervalIndexer<Polynomial> omegat, IntervalIndexer<Polynomial> anglet, IntervalIndexer<Polynomial> vt, IntervalIndexer<Polynomial> xt)
        {
            _disk = disk;
            _omegat = omegat;
            _anglet = anglet;
            _vt = vt;
            _xt = xt;
        }

        public AngularMomentumSimulationMoment GetMomentInTime(double t)
        {
            return new AngularMomentumSimulationMoment()
            {
                Disk = _disk,
                Angle = _anglet[t].Value.Evaluate(t),
                X = new AxisMovementParameters() { V= _vt[t].Value.Evaluate(t), D = _xt[t].Value.Evaluate(t) }
            };
        }
    }

    public class AngularMomentumSimulationMoment
    {
        public double Angle { get; set; }
        public SpecificMassEllipse Disk { get; set; }
        public AxisMovementParameters X { get; set; }
        public double Momentum { get; set; }
        public double AngularMomentum { get; set; }
        public double TranslationalKineticEnergy { get; set; }
        public double AngularKineticEnergy { get; set; }
        public double TotalKineticEnergy { get; set; }
    }
}
