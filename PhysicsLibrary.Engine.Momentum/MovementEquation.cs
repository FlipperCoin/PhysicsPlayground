using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace PhysicsLibrary.Engine.Momentum
{
    class MovementEquation
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

    internal class Polynomial
    {
        public double[] Coefficients { get; private set; }

        public Polynomial(params double[] coefficients)
        {
            Coefficients = coefficients.Reverse().SkipWhile(co => co == 0).Reverse().ToArray();
        }

        public Polynomial AntiDerivative(double arbitraryConstant = 0)
        {
            var newCoefficients = new double[Coefficients.Length + 1];
            newCoefficients[0] = arbitraryConstant;
            for (var i = 1; i < newCoefficients.Length; i++)
            {
                newCoefficients[i] = Coefficients[i - 1] * (1.0 / i);
            }

            return new Polynomial(newCoefficients);
        }

        public static Polynomial operator +(Polynomial p1, Polynomial p2)
        {
            var zipped = p1.Coefficients.Zip(p2.Coefficients, (first,second) => first+second).ToList();
            
            var (higherDim, lowerDim) = 
                p1.Coefficients.Length > p2.Coefficients.Length ? (p1, p2) : (p2, p1);
            zipped.AddRange(higherDim.Coefficients.Skip(lowerDim.Coefficients.Length));
            
            return new Polynomial(zipped.ToArray());
        }

        public static Polynomial operator +(Polynomial p1, double x)
        {
            if (x == 0) return p1.Clone();
            var newCo = p1.Coefficients.Length > 0 ? p1.Coefficients : new double[1];
            newCo[0] += x;

            return new Polynomial(newCo);
        }

        public Polynomial Clone()
        {
            return new Polynomial(Coefficients);
        }

        public static Polynomial operator +(double x, Polynomial p1) => p1 + x;
        public static Polynomial operator -(Polynomial p1, double x) => p1 + (-x);

        public double Evaluate(double x)
        {
            double sum = 0;
            for (var i = 0; i < Coefficients.Length; i++)
            {
                sum += Coefficients[i] * Math.Pow(x, i);
            }

            return sum;
        }

        public Polynomial Derivative()
        {
            if (Coefficients.Length == 0) return new Polynomial();

            var newCo = new double[Coefficients.Length - 1];
            for (var i = 1; i < Coefficients.Length; i++)
            {
                newCo[i - 1] = Coefficients[i] * i;
            }

            return new Polynomial(newCo);
        }

        public override string ToString()
        {
            return string.Join(" + ", Coefficients.Select((co, idx) => $"{co}x^{idx}"));
        }
    }
}
