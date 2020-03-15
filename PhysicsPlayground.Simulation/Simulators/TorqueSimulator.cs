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
            
            return new AngularMomentumSimulation(_disc, angleOverTime);
        }
    }

    public class AngularMomentumSimulation : ISimulation<AngularMomentumSimulationMoment>
    {
        private readonly SpecificMassEllipse _disc;
        private readonly Polynomial _angleOverTime;

        public AngularMomentumSimulation(SpecificMassEllipse disc, Polynomial angleOverTime)
        {
            _disc = disc;
            _angleOverTime = angleOverTime;
        }

        public AngularMomentumSimulationMoment GetMomentInTime(double t)
        {
            return new AngularMomentumSimulationMoment()
            {
                Disc = _disc,
                Angle = _angleOverTime.Evaluate(t)
            };
        }
    }

    public class AngularMomentumSimulationMoment
    {
        public double Angle { get; set; }
        public SpecificMassEllipse Disc { get; set; }
        public double Momentum { get; set; }
        public double AngularMomentum { get; set; }
        public double TranslationalKineticEnergy { get; set; }
        public double AngularKineticEnergy { get; set; }
        public double TotalKineticEnergy { get; set; }
    }
}
