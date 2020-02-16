using System;

namespace PhysicsPlayground.Simulation
{
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
}