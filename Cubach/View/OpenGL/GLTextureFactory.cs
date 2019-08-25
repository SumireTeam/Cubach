using System.Drawing;

namespace Cubach.View.OpenGL
{
    public class GLTextureFactory : ITextureFactory<GLTexture>
    {
        public GLTexture Create(Bitmap bitmap, bool withAlpha = false)
        {
            var texture = new GLTexture();
            texture.SetImage(bitmap, withAlpha);
            return texture;
        }
    }
}
