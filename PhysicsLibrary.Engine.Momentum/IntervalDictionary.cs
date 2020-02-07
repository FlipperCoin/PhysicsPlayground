using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Double;

namespace PhysicsLibrary.Engine.Momentum
{
    public enum EndpointType
    {
        Unbounded,
        Open,
        Closed
    }

    public static class Endpoints
    {
        public static (double, EndpointType) Unbounded => (NaN, EndpointType.Unbounded);
    }

    class IntervalIndexer<T>
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
                    if ((curMin != minimum && !(IsNaN(curMin) && IsNaN(minimum))) || (curMinType == EndpointType.Closed && minimumType == EndpointType.Open))
                    {
                        newIntervals.Add(
                            (
                                ((curMin,curMinType),(minimum, minimumType == EndpointType.Closed ? EndpointType.Open : EndpointType.Closed))
                                ,minRangeVal
                            )
                        );
                    }
                    newIntervals.Add((interval,value));
                    if ((curMax != maximum && !(IsNaN(curMax) && IsNaN(maximum))) || (curMaxType == EndpointType.Closed && maximumType == EndpointType.Open))
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

    internal interface IIntervalIndexer<T>
    {
        T this[double t] { get; }
        void AddInterval(Interval interval, T value);
    }

    abstract class IntervalEndpointBase
    {
        public abstract bool InRangeBelow(double t);
        public abstract bool InRangeAbove(double t);
        public abstract bool Smaller(double t);
        public abstract bool Larger(double t);
        public abstract bool Equals(double t);
        public static bool operator <(double t, IntervalEndpointBase e) => e.Larger(t);
        public static bool operator <=(double t, IntervalEndpointBase e) => t < e || e.Equals(t);

        public static bool operator >(double t, IntervalEndpointBase e) => e.Smaller(t);
        public static bool operator >=(double t, IntervalEndpointBase e) => t < e || e.Equals(t);
        public static bool operator <(IntervalEndpointBase e, double t) => t > e;
        public static bool operator <=(IntervalEndpointBase e, double t) => t >= e;

        public static bool operator >(IntervalEndpointBase e, double t) => t < e;
        public static bool operator >=(IntervalEndpointBase e, double t) => t <= e;
        //public static bool operator <(IntervalEndpointBase e1, IntervalEndpointBase e2) => t > e;
        //public static bool operator <=(IntervalEndpointBase e1, IntervalEndpointBase e2) => t >= e;

        //public static bool operator >(IntervalEndpointBase e1, IntervalEndpointBase e2) => t < e;
        //public static bool operator >=(IntervalEndpointBase e1, IntervalEndpointBase e2) => t <= e;
    }

    class BoundedEndpointBase : IntervalEndpointBase
    {
        public double Value { get; }

        public BoundedEndpointType Type { get; }

        public BoundedEndpointBase(double value, BoundedEndpointType type)
        {
            Value = value;
            Type = type;
        }

        public override bool InRangeBelow(double t)
        {
            return Type switch
            {
                BoundedEndpointType.Open => t < this,
                BoundedEndpointType.Closed => t <= this,
                _ => throw new Exception()
            };
        }

        public override bool InRangeAbove(double t)
        {
            return Type switch
            {
                BoundedEndpointType.Open => t > this,
                BoundedEndpointType.Closed => t >= this,
                _ => throw new Exception()
            };
        }

        public override bool Smaller(double t) => t < Value;

        public override bool Larger(double t) => t > Value;

        public override bool Equals(double t) => t == Value;
    }

    internal enum BoundedEndpointType
    {
        Open,
        Closed
    }

    class UnboundedEndpointBase : IntervalEndpointBase
    {
        public bool Sign { get; }

        public UnboundedEndpointBase(bool sign)
        {
            Sign = sign;
        }

        public override bool InRangeBelow(double t) => t < this;

        public override bool InRangeAbove(double t) => t > this;

        public override bool Smaller(double t)
        {
            return !Sign;
        }

        public override bool Larger(double t)
        {
            return Sign;
        }

        public override bool Equals(double t) => false;
    }
}
