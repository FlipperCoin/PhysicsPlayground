namespace PhysicsPlayground.Runtime
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