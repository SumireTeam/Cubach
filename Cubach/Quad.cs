namespace Cubach
{
    public struct Quad<T>
    {
        public readonly T LeftBottom;
        public readonly T RightBottom;
        public readonly T LeftTop;
        public readonly T RightTop;

        public Quad(T leftBottom, T rightBottom, T leftTop, T rightTop)
        {
            LeftBottom = leftBottom;
            RightBottom = rightBottom;
            LeftTop = leftTop;
            RightTop = rightTop;
        }

        public Quad(T[,] points) : this(
            points[0, 0], points[1, 0],
            points[0, 1], points[1, 1]
        ) { }

        public Quad(Line<T> bottom, Line<T> top) : this(
            bottom.Start, bottom.End,
            top.Start, top.End
        ) { }

        public T[,] Points => new[,] {
            {LeftBottom, RightBottom},
            {LeftTop, RightTop},
        };

        public Line<T> Bottom => new Line<T>(LeftBottom, RightBottom);
        public Line<T> Top => new Line<T>(LeftTop, RightTop);
        public Line<T> Left => new Line<T>(LeftTop, LeftBottom);
        public Line<T> Right => new Line<T>(RightTop, RightBottom);
    }
}
