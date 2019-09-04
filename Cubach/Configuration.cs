namespace Cubach
{
    public class Configuration
    {
        public readonly string FontFamily;
        public readonly int FontSize;
        public readonly string SavePath;
        public readonly int RenderDistance;

        public Configuration(string fontFamily, int fontSize, string savePath, int renderDistance)
        {
            FontFamily = fontFamily;
            FontSize = fontSize;
            SavePath = savePath;
            RenderDistance = renderDistance;
        }
    }
}
