namespace PhysicsPlayground.Simulation
{
    public interface IRunnable
    {
        RunState RunState { get; }
        void Start();

        void Pause();

        void Resume();

        void Stop();
    }
}