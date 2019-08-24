using OpenTK;
using System.Collections.Generic;

namespace Cubach.View.OpenGL
{
    public class GLTextRenderer : TextRenderer<Texture>
    {
        public GLTextRenderer(ISpriteBatch<Texture> spriteBatch) : base(spriteBatch) { }

        public override FontPage<Texture> CreateFontPage(string fontFamily, int emSize, int page)
        {
            Texture texture = new Texture();
            using (var bitmap = CreateFontPageBitmap(fontFamily, emSize, page, out Dictionary<char, Vector2> sizes))
            {
                texture.SetImage(bitmap, true);
                return new FontPage<Texture>(texture, sizes);
            }
        }
    }
}
