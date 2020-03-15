using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhysicsPlayground.Math;

namespace PhysicsPlayground.Simulation.Simulators
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
            var circumferenceByR = new Polynomial(0, 2 * System.Math.PI);
            var tMass = (circumferenceByR * _disk.SpecificMassByR).DefiniteIntegral(_disk.Radius);
            var rSquared = new Polynomial(0, 0, 1);
            var iZ = ((circumferenceByR * _disk.SpecificMassByR) * rSquared).DefiniteIntegral(_disk.Radius);
            var friction = -(_coOfKineticFriction * tMass * g0);
            
            var torque = _disk.Radius * friction;
            var angularAcceleration = torque / iZ;
            var omega = (new Polynomial(angularAcceleration)).AntiDerivative(_omega0);

            var a = friction / tMass;
            var v = (new Polynomial(a)).AntiDerivative(_v0);

            var vOnSurface = v + omega * _disk.Radius;

            var tf = t2;
            bool foundTf = true;
            try
            {
                tf = vOnSurface.Roots(0, t1, t2).First();
            }
            catch (InvalidOperationException)
            {
                foundTf = false;
            }

            var slipInterval = (Endpoints.Closed(t1), Endpoints.Closed(tf));
            var noSlipInterval = (Endpoints.Open(tf), Endpoints.Unbounded);

            var vt = new IntervalIndexer<Polynomial>();
            vt.AddInterval(slipInterval, v);

            var xt = new IntervalIndexer<Polynomial>();
            xt.AddInterval(slipInterval, v.AntiDerivative());

            var omegat = new IntervalIndexer<Polynomial>();
            omegat.AddInterval(slipInterval, omega);

            var anglet = new IntervalIndexer<Polynomial>();
            anglet.AddInterval(slipInterval, omega.AntiDerivative());

            if (foundTf)
            {
                vt.AddInterval(
                    noSlipInterval, 
                    new Polynomial(v.Evaluate(tf))
                );
                xt.AddInterval(
                    noSlipInterval,
                    new Polynomial(v.AntiDerivative().Evaluate(tf), v.Evaluate(tf))
                        .Offset(-tf)
                );

                omegat.AddInterval(
                    noSlipInterval, 
                    new Polynomial(omega.Evaluate(tf))
                );
                anglet.AddInterval(
                    noSlipInterval,
                    new Polynomial(omega.AntiDerivative().Evaluate(tf), omega.Evaluate(tf))
                        .Offset(-tf)
                );
            }

            return new AngularMomentumSimulation(_disk, omegat, anglet, vt, xt);
        }
    }
}
