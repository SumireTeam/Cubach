namespace Cubach
{
    public class Configuration
    {
        public readonly string FontFamily;
        public readonly int FontSize;
        public readonly string SavePath;
        public readonly int RenderDistance;
        public readonly int Width;
        public readonly int Height;
        public readonly bool Fullscreen;

        public Configuration(string fontFamily, int fontSize, string savePath, int renderDistance,
            int width, int height, bool fullscreen)
        {
            FontFamily = fontFamily;
            FontSize = fontSize;
            SavePath = savePath;
            RenderDistance = renderDistance;
            Width = width;
            Height = height;
            Fullscreen = fullscreen;
        }
    }
}
