using System;

namespace PhysicsPlayground.Simulation
{
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
                RunState.Running =>
                    (_baseTime + TimeSpan.FromMilliseconds(DateTime.Now.Subtract(_startTime).TotalMilliseconds*Speed)),
                RunState.Paused => _baseTime,
                RunState.Stopped => _t0,
                _ => throw new Exception("Internal Error")
            };
        }

        private double _speed;
        public double Speed
        {
            get => _speed;
            set
            {
                _baseTime = Time;
                _startTime = DateTime.Now;
                _speed = value;
            }
        }

        public RunTime(double t0, double speed)
        {
            RunState = RunState.Stopped;
            _speed = speed;
            _t0 = TimeSpan.FromSeconds(t0);
            _baseTime = _t0;
        }

        public RunTime(double speed = 1) : this(0, speed) { }

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
            _startTime = DateTime.Now;
            RunState = RunState.Running;
        }

        public void Stop()
        {
            _baseTime = _t0;
            RunState = RunState.Stopped;
        }
    }
}