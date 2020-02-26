namespace PhysicsPlayground.Simulation
{
    internal interface IIntervalIndexer<T>
    {
        T this[double t] { get; }
        void AddInterval(Interval interval, T value);
    }
}