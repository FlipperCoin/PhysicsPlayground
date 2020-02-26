using System.Collections.Generic;

namespace PhysicsPlayground.Simulation
{
    class GenericEndpointIntervalIndexer<T> : IIntervalIndexer<T> where T : class 
    {
        private readonly List<(IntervalEndpointBase, T)> _endpoints;

        public GenericEndpointIntervalIndexer()
        {
            _endpoints = new List<(IntervalEndpointBase, T)>
            {
                (new UnboundedEndpointBase(true), null)
            };
        }

        public T this[double t]
        {
            get
            {
                foreach (var (endpoint, returnValue) in _endpoints)
                {
                    if (endpoint.InRangeBelow(t)) return returnValue;
                }

                return null;
            }
        }

        public void AddInterval(Interval interval, T value)
        {
        }
    }
}