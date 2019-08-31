namespace Cubach
{
    public class Configuration
    {
        public readonly string FontFamily;
        public readonly int FontSize;
        public readonly string SavePath;

        public Configuration(string fontFamily, int fontSize, string savePath)
        {
            FontFamily = fontFamily;
            FontSize = fontSize;
            SavePath = savePath;
        }
    }
}
