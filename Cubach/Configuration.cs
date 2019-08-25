namespace Cubach
{
    public class Configuration
    {
        public readonly string FontFamily;
        public readonly int FontSize;

        public Configuration(string fontFamily, int fontSize)
        {
            FontFamily = fontFamily;
            FontSize = fontSize;
        }
    }
}
