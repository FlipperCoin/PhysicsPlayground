using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using Polynomial = PhysicsPlayground.Math.Polynomial;

namespace PhysicsPlayground.Simulation
{
    public interface ISimulation
    {
        IEnumerable<(double, double)> GetCoordinates(double t);
    }

    public interface ISimulator
    {
        /// <summary>
        /// Generates a simulation in the provided closed interval
        /// </summary>
        /// <param name="t1">starting point</param>
        /// <param name="t2">ending point</param>
        /// <returns>the generated simulation</returns>
        Task<ISimulation> GenerateSimulationAsync(double t1, double t2);

        /// <summary>
        /// Generates a simulation in the provided closed interval
        /// </summary>
        /// <param name="t1">starting point</param>
        /// <param name="t2">ending point</param>
        /// <returns>the generated simulation</returns>
        ISimulation GenerateSimulation(double t1, double t2);
    }

    public abstract class SyncSimulator : ISimulator
    {
        public Task<ISimulation> GenerateSimulationAsync(double t1, double t2) =>
            Task.Run(() => GenerateSimulation(t1, t2));

        public abstract ISimulation GenerateSimulation(double t1, double t2);
    }

    public abstract class AsyncSimulator : ISimulator
    {
        public abstract Task<ISimulation> GenerateSimulationAsync(double t1, double t2);

        ISimulation ISimulator.GenerateSimulation(double t1, double t2) =>
            GenerateSimulationAsync(t1, t2).Result;

    }

    public class ConservationOfMomentumSimulator : SyncSimulator
    {
        private readonly GridParams _grid;
        private readonly IEnumerable<(MassObject, MovementParameters2)> _objectsAndMovementParameters;

        public ConservationOfMomentumSimulator(GridParams grid, IEnumerable<(MassObject, MovementParameters2)> objectsAndMovementParameters)
        {
            _grid = grid;
            _objectsAndMovementParameters = objectsAndMovementParameters;
        }

        public override ISimulation GenerateSimulation(double t1, double t2)
        {
            IEnumerable<MovementEquation> movementEquations = _objectsAndMovementParameters
                .Select(obj =>
                {
                    var (massObject, (x, y)) = obj;

                    var axisParams = x;
                    var (axisMin, axisMax) = (0, _grid.X);
                    
                    var xEquations = BuildAxisEquations(t1, t2, x, 0, _grid.X);
                    var yEquations = BuildAxisEquations(t1, t2, y, 0, _grid.Y);

                    return new MovementEquation(xEquations, yEquations);
                }).ToList();

            return new Simulation(movementEquations);
        }

        private static IntervalIndexer<Polynomial> BuildAxisEquations(double t1, double t2, InitialMovementParameters axisParams,
            int axisMin, double axisMax)
        {
            var pol = MovementEquation.GetPolynomialMovementEquation(axisParams);
            var axisEquations = new IntervalIndexer<Polynomial>();
            axisEquations.AddInterval((Endpoints.Unbounded, Endpoints.Unbounded), pol);

            var t = t1;
            while (t < t2)
            {
                // Using root > t instead of >= means that putting the object on the edge from the start
                // means that it will go the direction it was set to
                var rootsWithMin = pol.Roots(axisMin).Where(root => root.CompareTo(t,5e-6) == 1 && root <= t2);
                var rootsWithMax = pol.Roots(axisMax).Where(root => root.CompareTo(t, 5e-6) == 1 && root <= t2);

                var roots = rootsWithMin.Concat(rootsWithMax);
                if (!roots.Any())
                {
                    break;
                }

                var minRoot = roots.Min();

                axisParams = new InitialMovementParameters()
                {
                    D0 = pol.Evaluate(minRoot), V0 = -pol.Derivative().Evaluate(minRoot), A0 = pol.Derivative().Derivative().Evaluate(minRoot), T0 = minRoot
                };
                pol = MovementEquation.GetPolynomialMovementEquation(axisParams);
                axisEquations.AddInterval((Endpoints.Closed(minRoot), Endpoints.Open(t2)), pol);
                t = minRoot;
            }

            return axisEquations;
        }
    }

    public class Simulation : ISimulation
    {
        private readonly IEnumerable<MovementEquation> _movementEquations;

        public Simulation(IEnumerable<MovementEquation> movementEquations)
        {
            _movementEquations = movementEquations;
        }

        public IEnumerable<(double, double)> GetCoordinates(double t) =>
            _movementEquations.Select(equation => equation.GetLocationInTime(t));
    }

    public class SimulationRunner : IObjectsStateProvider
    {
        private readonly ISimulation _simulation;
        private readonly ITimeProvider _timeProvider;

        public SimulationRunner(ISimulation simulation, ITimeProvider timeProvider)
        {
            _simulation = simulation;
            _timeProvider = timeProvider;
        }

        public IEnumerable<(double x, double y)> GetCoordinates()
        {
            return _simulation.GetCoordinates(_timeProvider.Time.TotalSeconds);
        }
    }

    public interface ITimeProvider
    {
        TimeSpan Time { get; }
    }

    public enum RunState
    {
        None = 0,
        Running = 1,
        Paused = 2,
        Stopped = 3
    }

    public interface IRunnable
    {
        RunState RunState { get; }
        void Start();

        void Pause();

        void Resume();

        void Stop();
    }

    public interface IRunTime : IRunnable, ITimeProvider { }

    public class RunTime : IRunTime
    {
        private readonly TimeSpan _t0;
        private TimeSpan _baseTime;
        private DateTime _startTime;
        public RunState RunState { get; private set; }

        public RunTime()
        {
            RunState = RunState.Stopped;
        }

        public TimeSpan Time
        {
            get => RunState switch
            {
                RunState.Running => _baseTime + DateTime.Now.Subtract(_startTime),
                RunState.Paused => _baseTime,
                RunState.Stopped => _t0,
                _ => throw new Exception("Internal Error")
            };
        }

        public RunTime(double t0 = 0)
        {
            _t0 = TimeSpan.FromSeconds(t0);
            _baseTime = _t0;
        }

        public void Start()
        {
            Resume();
        }

        public void Pause()
        {
            _baseTime = Time;
            RunState = RunState.Paused;
        }

        public void Resume()
        {
            RunState = RunState.Running;
            _startTime = DateTime.Now;
        }

        public void Stop()
        {
            _baseTime = _t0;
            RunState = RunState.Stopped;
        }
    }

    public interface IObjectsStateProvider
    {
        IEnumerable<(double x, double y)> GetCoordinates();
    }
}
