using System.Drawing;

namespace Cubach.View
{
    public interface ITextureFactory<TTexture> where TTexture : ITexture
    {
        TTexture Create(Bitmap bitmap, bool withAlpha = false);
    }
}
