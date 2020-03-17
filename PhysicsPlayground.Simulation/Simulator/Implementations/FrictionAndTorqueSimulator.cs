using System;
using System.Linq;
using MathNet.Numerics;
using PhysicsPlayground.DataStructures;
using Polynomial = PhysicsPlayground.Math.Polynomial;

namespace PhysicsPlayground.Simulation.Simulator.Implementations
{
    public class FrictionAndTorqueSimulator : SyncSimulator<AngularMomentumSimulationMoment>
    {
        private static readonly double g0 = 9.807;

        private readonly SpecificMassEllipse _disk;
        private readonly double _v0;
        private readonly double _omega0;
        private readonly double _coOfKineticFriction;

        public FrictionAndTorqueSimulator(SpecificMassEllipse disk, double v0, double omega0, double coOfKineticFriction)
        {
            _disk = disk;
            _v0 = v0;
            _omega0 = omega0;
            _coOfKineticFriction = coOfKineticFriction;
        }

        public override ISimulation<AngularMomentumSimulationMoment> GenerateSimulation(double t1, double t2)
        {
            var vt = new IntervalIndexer<Polynomial>();
            var xt = new IntervalIndexer<Polynomial>();
            var omegat = new IntervalIndexer<Polynomial>();
            var anglet = new IntervalIndexer<Polynomial>();

            var v = new Polynomial(_v0);
            var omega = new Polynomial(_omega0);

            var vOnSurface = v + omega * _disk.Radius;

            var t = t1;

            var vOnSurface0 = vOnSurface.Evaluate(t);

            // There's friction that acts on the disk
            if (vOnSurface0.CompareTo(0,6e-5) != 0)
            {
                var circumferenceByR = new Polynomial(0, 2 * System.Math.PI);
                var tMass = (circumferenceByR * _disk.SpecificMassByR).DefiniteIntegral(_disk.Radius);
                var rSquared = new Polynomial(0, 0, 1);
                var iZ = ((circumferenceByR * _disk.SpecificMassByR) * rSquared).DefiniteIntegral(_disk.Radius);
                var friction = (-System.Math.Sign(vOnSurface0)) * (_coOfKineticFriction * tMass * g0);

                var torque = _disk.Radius * friction;
                var angularAcceleration = torque / iZ;
                omega = (new Polynomial(angularAcceleration)).AntiDerivative(_omega0);

                var a = friction / tMass;
                v = (new Polynomial(a)).AntiDerivative(_v0);

                vOnSurface = v + omega * _disk.Radius;

                var tf = t2;
                try
                {
                    tf = vOnSurface.Roots(0, t1, t2).First();
                }
                catch (InvalidOperationException)
                {
                }

                var slipInterval = (Endpoints.Closed(t), Endpoints.Open(tf));

                vt.AddInterval(slipInterval, v);
                xt.AddInterval(slipInterval, v.AntiDerivative());
                omegat.AddInterval(slipInterval, omega);
                anglet.AddInterval(slipInterval, omega.AntiDerivative());

                t = tf;
            }

            if (t < t2)
            {
                var noSlipInterval = (Endpoints.Closed(t), Endpoints.Unbounded);

                vt.AddInterval(
                    noSlipInterval, 
                    new Polynomial(v.Evaluate(t))
                );
                xt.AddInterval(
                    noSlipInterval,
                    new Polynomial(v.AntiDerivative().Evaluate(t), v.Evaluate(t))
                        .Offset(-t)
                );

                omegat.AddInterval(
                    noSlipInterval, 
                    new Polynomial(omega.Evaluate(t))
                );
                anglet.AddInterval(
                    noSlipInterval,
                    new Polynomial(omega.AntiDerivative().Evaluate(t), omega.Evaluate(t))
                        .Offset(-t)
                );
            }

            return new AngularMomentumSimulation(_disk, omegat, anglet, vt, xt);
        }
    }
}
