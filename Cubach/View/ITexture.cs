using System;
using System.Drawing;

namespace Cubach.View
{
    public interface ITexture : IDisposable
    {
        void SetImage(Bitmap image, bool withAlpha = false);
        void Bind(int slot = 0);
    }
}
