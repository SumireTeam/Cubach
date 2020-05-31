using System;
using System.Collections.Generic;

namespace Cubach.View
{
    public interface ITextureAtlas
    {
        TextureRegion GetRegion(string name);
    }

    public class TextureAtlas<TTexture> : ITextureAtlas, IDisposable where TTexture : ITexture
    {
        public TTexture Texture { get; }

        private readonly Dictionary<string, TextureRegion> regions;

        public TextureAtlas(TTexture texture, Dictionary<string, TextureRegion> regions)
        {
            Texture = texture;
            this.regions = regions;
        }

        public TextureRegion GetRegion(string name)
        {
            return regions[name];
        }

        public void Dispose()
        {
            (Texture as IDisposable)?.Dispose();
        }
    }
}
