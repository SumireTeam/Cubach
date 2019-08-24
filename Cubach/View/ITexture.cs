using System.Drawing;

namespace Cubach.View
{
    public interface ITexture
    {
        void SetImage(Bitmap image, bool withAlpha = false);
    }
}
