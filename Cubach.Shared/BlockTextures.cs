namespace Cubach.Shared
{
    public struct BlockTextures
    {
        public readonly string Rear;
        public readonly string Front;
        public readonly string Left;
        public readonly string Right;
        public readonly string Bottom;
        public readonly string Top;

        public BlockTextures(string rear, string front, string left, string right, string bottom, string top)
        {
            Rear = rear;
            Front = front;
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }

        public BlockTextures(string bottom, string side, string top) : this(side, side, side, side, bottom, top) { }
        public BlockTextures(string texture) : this(texture, texture, texture, texture, texture, texture) { }
    }
}
