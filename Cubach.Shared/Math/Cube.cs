namespace Cubach.Shared.Math
{
    public struct Cube<T>
    {
        public readonly T LeftRearBottom;
        public readonly T RightRearBottom;
        public readonly T LeftFrontBottom;
        public readonly T RightFrontBottom;
        public readonly T LeftRearTop;
        public readonly T RightRearTop;
        public readonly T LeftFrontTop;
        public readonly T RightFrontTop;

        public Cube(T leftRearBottom, T rightRearBottom, T leftFrontBottom, T rightFrontBottom,
            T leftRearTop, T rightRearTop, T leftFrontTop, T rightFrontTop)
        {
            LeftRearBottom = leftRearBottom;
            RightRearBottom = rightRearBottom;
            LeftFrontBottom = leftFrontBottom;
            RightFrontBottom = rightFrontBottom;
            LeftRearTop = leftRearTop;
            RightRearTop = rightRearTop;
            LeftFrontTop = leftFrontTop;
            RightFrontTop = rightFrontTop;
        }

        public Cube(T[,,] points) : this(
            points[0, 0, 0], points[1, 0, 0],
            points[0, 1, 0], points[1, 1, 0],
            points[0, 0, 1], points[1, 0, 1],
            points[0, 1, 1], points[1, 1, 1]
        )
        { }

        public T[,,] Points => new[, ,] {
            {
                {LeftRearBottom, RightRearBottom},
                {LeftFrontBottom, RightFrontBottom},
            }, {
                {LeftRearTop, RightRearTop},
                {LeftFrontTop, RightFrontTop},
            },
        };

        public Quad<T> Bottom => new Quad<T>(LeftRearBottom, RightRearBottom, LeftFrontBottom, RightFrontBottom);
        public Quad<T> Top => new Quad<T>(LeftRearTop, RightRearTop, LeftFrontTop, RightFrontTop);
        public Quad<T> Rear => new Quad<T>(LeftRearBottom, RightRearBottom, LeftRearTop, RightRearTop);
        public Quad<T> Front => new Quad<T>(LeftFrontBottom, RightFrontBottom, LeftFrontTop, RightFrontTop);
        public Quad<T> Left => new Quad<T>(LeftRearBottom, LeftFrontBottom, LeftRearTop, LeftFrontTop);
        public Quad<T> Right => new Quad<T>(RightRearBottom, RightFrontBottom, RightRearTop, RightFrontTop);
    }
}
