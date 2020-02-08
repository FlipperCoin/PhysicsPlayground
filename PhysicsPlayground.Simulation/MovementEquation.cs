﻿using PhysicsPlayground.Math;

namespace PhysicsPlayground.Simulation
{
    public class MovementEquation
    {
        private readonly IntervalIndexer<Polynomial> _xEquations;
        private readonly IntervalIndexer<Polynomial> _yEquations;

        public MovementEquation(InitialMovementParameters x, InitialMovementParameters y)
        {
            _xEquations = new IntervalIndexer<Polynomial>();
            UpdateXEquations(new Interval()
            {
                Minimum = IntervalEndpoint.Unbounded, 
                Maximum = IntervalEndpoint.Unbounded
            }, x);
            _yEquations = new IntervalIndexer<Polynomial>();
            UpdateYEquations(new Interval()
            {
                Minimum = IntervalEndpoint.Unbounded,
                Maximum = IntervalEndpoint.Unbounded
            }, y);
        }

        public (double x, double y) GetLocationInTime(double t)
        {
            var (x, y) = GetEquationsInTime(t);
            
            return (x.Evaluate(t), y.Evaluate(t));
        }

        public (double vx, double vy) GetSpeedInTime(double t)
        {
            var (x, y) = GetEquationsInTime(t);
            return (x.Derivative().Evaluate(t), y.Derivative().Evaluate(t));
        }
        public (double ax, double ay) GetAccelerationInTime(double t)
        {
            var (x, y) = GetEquationsInTime(t);
            return (x.Derivative().Derivative().Evaluate(t), y.Derivative().Derivative().Evaluate(t));
        }

        private (Polynomial x, Polynomial y) GetEquationsInTime(double t)
        {
            return (_xEquations[t].Value, _yEquations[t].Value);
        }

        public void UpdateXEquations(Interval timeInterval, InitialMovementParameters updatedEquation)
        {
            UpdateEquations(_xEquations, timeInterval, updatedEquation);
        }

        public void UpdateYEquations(Interval timeInterval, InitialMovementParameters updatedEquation)
        {
            UpdateEquations(_yEquations, timeInterval, updatedEquation);
        }

        private void UpdateEquations(IntervalIndexer<Polynomial> equations,
            Interval timeInterval,
            InitialMovementParameters updatedEquation)
        {
            var pol = GetPolynomialMovementEquation(
                updatedEquation.A0,
                updatedEquation.V0,
                updatedEquation.D0,
                updatedEquation.T0);


            equations.AddInterval(
                ((timeInterval.Minimum.Value,timeInterval.Minimum.Type),(timeInterval.Maximum.Value,timeInterval.Maximum.Type)),
                pol);
        }

        private Polynomial GetPolynomialMovementEquation(InitialMovementParameters movementParameters)
        {
            return GetPolynomialMovementEquation(movementParameters.A0, 
                movementParameters.V0, 
                movementParameters.D0,
                movementParameters.T0);
        }

        private Polynomial GetPolynomialMovementEquation(double a0, double v0, double d0, double t0)
        {
            var a = new Polynomial(a0);
            var d =
                (a.AntiDerivative() + v0)
                .AntiDerivative();
            d -= d.Evaluate(t0);
            d += d0;

            return d;
        }
    }
}