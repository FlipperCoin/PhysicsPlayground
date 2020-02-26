using System;
using System.Collections.Generic;
using System.Linq;

namespace PhysicsPlayground.Simulation
{
    public class IntervalIndexer<T>
    {
        private List<(((double, EndpointType), (double, EndpointType)), Optional<T>)> _intervals;

        public IntervalIndexer()
        {
            _intervals = new List<(((double, EndpointType), (double, EndpointType)), Optional<T>)>
            {
                ((Endpoints.Unbounded,Endpoints.Unbounded), new Optional<T>())
            };
        }

        public Optional<T> this[double t]
        {
            get
            {
                foreach (var (interval, returnValue) in _intervals)
                {
                    if (InInterval(interval, t)) return returnValue;
                }

                return new Optional<T>();
            }
        }

        public void AddInterval(((double, EndpointType), (double, EndpointType)) interval, T value)
        {
            for (var i = _intervals.Count - 1; i >= 0; i--)
            {
                var ((minimum, minimumType), (maximum, maximumType)) = interval;

                var (((curMin, curMinType), (_, _)), minRangeVal) = _intervals[i];

                var reachedMin = (minimumType, nextMinType: curMinType) switch
                {
                    (EndpointType.Closed, EndpointType.Closed) => curMin <= minimum,
                    (EndpointType.Closed, EndpointType.Open) => curMin < minimum,
                    (EndpointType.Open, EndpointType.Closed) => curMin <= minimum,
                    (EndpointType.Open, EndpointType.Open) => curMin <= minimum,
                    (_, EndpointType.Unbounded) => true,
                    (EndpointType.Unbounded, _) => false,
                    _ => throw new Exception()
                };

                if (!reachedMin)
                {
                    if (i == 0)
                        break;
                    
                    continue;
                }

                for (var j = i; j < _intervals.Count; j++)
                {
                    var (((_, _), (curMax, curMaxType)), maxRangeVal) = _intervals[i];

                    var reachedMax = (maximumType, nextMaxType: curMaxType) switch
                    {
                        (EndpointType.Closed, EndpointType.Closed) => maximum <= curMax,
                        (EndpointType.Closed, EndpointType.Open) => maximum < curMax,
                        (EndpointType.Open, EndpointType.Closed) => maximum <= curMax,
                        (EndpointType.Open, EndpointType.Open) => maximum <= curMax,
                        (_, EndpointType.Unbounded) => true,
                        (EndpointType.Unbounded, _) => false,
                        _ => throw new Exception()
                    };

                    if (!reachedMax)
                    {
                        if (j == _intervals.Count)
                            break;

                        continue;
                    }

                    var newIntervals = new List<(((double, EndpointType), (double, EndpointType)), Optional<T>)>();
                    newIntervals.AddRange(_intervals.Take(i));
                    if ((curMin != minimum && !(Double.IsNaN(curMin) && Double.IsNaN(minimum))) || (curMinType == EndpointType.Closed && minimumType == EndpointType.Open))
                    {
                        newIntervals.Add(
                            (
                                ((curMin,curMinType),(minimum, minimumType == EndpointType.Closed ? EndpointType.Open : EndpointType.Closed))
                                ,minRangeVal
                            )
                        );
                    }
                    newIntervals.Add((interval,value));
                    if ((curMax != maximum && !(Double.IsNaN(curMax) && Double.IsNaN(maximum))) || (curMaxType == EndpointType.Closed && maximumType == EndpointType.Open))
                    {
                        newIntervals.Add(
                            (
                                ((maximum, maximumType == EndpointType.Closed ? EndpointType.Open : EndpointType.Closed), (curMax, curMaxType))
                                ,maxRangeVal 
                            )
                        );
                    }
                    newIntervals.AddRange(_intervals.Skip(j + 1));

                    _intervals = newIntervals.ToList();
                    return;
                }
            }
        }

        private bool InInterval(((double, EndpointType), (double, EndpointType)) interval, double t)
        {
            var ((minimum, minimumType), (maximum, maximumType)) = interval;
            var above = minimumType switch
            {
                EndpointType.Unbounded => true,
                EndpointType.Open => t > minimum,
                EndpointType.Closed => t >= minimum,
                _ => throw new Exception()
            };

            if (!above) return false;

            var below = maximumType switch
            {
                EndpointType.Unbounded => true,
                EndpointType.Open => t < maximum,
                EndpointType.Closed => t <= maximum,
                _ => throw new Exception()
            };

            return below;
        }
    }
}