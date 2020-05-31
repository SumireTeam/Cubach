using System.Collections.Generic;

namespace Cubach.Shared.Math
{
    public struct Line<T>
    {
        public readonly T Start;
        public readonly T End;

        public Line(T start, T end)
        {
            Start = start;
            End = end;
        }

        public Line(IReadOnlyList<T> points) : this(points[0], points[1]) { }

        public T[] Points => new[] { Start, End };
    }
}
