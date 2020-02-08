using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    var (massObject, movementParams) = obj; 

                });

            return new Simulation(movementEquations);
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
            return _simulation.GetCoordinates(_timeProvider.Time.Seconds);
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

        public RunTime(double t0)
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
