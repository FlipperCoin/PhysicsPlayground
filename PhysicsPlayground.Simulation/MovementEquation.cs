using PhysicsPlayground.DataStructures;
using PhysicsPlayground.Math;

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
                LowerEndpoint = Endpoints.Unbounded,
                UpperEndpoint = Endpoints.Unbounded
            }, x);
            _yEquations = new IntervalIndexer<Polynomial>();
            UpdateYEquations(new Interval()
            {
                LowerEndpoint = Endpoints.Unbounded,
                UpperEndpoint = Endpoints.Unbounded
            }, y);
        }

        public MovementEquation(IntervalIndexer<Polynomial> xEquations, IntervalIndexer<Polynomial> yEquations)
        {
            _xEquations = xEquations;
            _yEquations = yEquations;
        }

        public MovementEquation()
        {
            
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

        public (Polynomial x, Polynomial y) GetEquationsInTime(double t)
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
                (timeInterval.LowerEndpoint, timeInterval.UpperEndpoint),
                pol);
        }

        public static Polynomial GetPolynomialMovementEquation(InitialMovementParameters movementParameters)
        {
            return GetPolynomialMovementEquation(movementParameters.A0, 
                movementParameters.V0, 
                movementParameters.D0,
                movementParameters.T0);
        }

        public static Polynomial GetPolynomialMovementEquation(double a0, double v0, double d0, double t0)
        {
            var a = new Polynomial(a0);
            var d = a.AntiDerivative(v0).AntiDerivative(d0).Offset(-t0);

            return d;
        }
    }
}
