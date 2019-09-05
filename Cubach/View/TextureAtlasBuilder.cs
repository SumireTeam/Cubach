using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using OpenTK;

namespace Cubach.View
{
    public class TextureAtlasBuilder<TTexture> : IDisposable where TTexture : ITexture
    {
        private readonly ITextureFactory<TTexture> textureFactory;
        private readonly Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

        public TextureAtlasBuilder(ITextureFactory<TTexture> textureFactory) => this.textureFactory = textureFactory;

        public void AddImage(string name, Bitmap image) => images[name] = image;

        public TextureAtlas<TTexture> Build()
        {
            var regions = new Dictionary<string, TextureRegion>();

            // For now just store all images in a row. Should work fine for a small amount of small images.
            // TODO: implement a packing algorithm to store images in a rectangle of a minimum size.

            // Image must be at least 1x1 pixel.
            var targetWidth = Math.Max(images.Select(kv => kv.Value.Width).Sum(), 1);
            var targetHeight = images.Count > 0 ? images.Select(kv => kv.Value.Height).Max() : 1;
            using (var targetImage = new Bitmap(targetWidth, targetHeight))
            using (var graphics = Graphics.FromImage(targetImage)) {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.Clear(Color.Transparent);

                var x = 0;
                foreach (var kv in images) {
                    var image = kv.Value;
                    var width = image.Width;
                    var height = image.Height;
                    var uvMin = new Vector2(x / (float) targetWidth, 0);
                    var uvMax = uvMin + new Vector2(width / (float) targetWidth, height / (float) targetHeight);
                    regions.Add(kv.Key, new TextureRegion(uvMin, uvMax));

                    graphics.DrawImage(image, x, 0, width, height);
                    x += width;
                }

                var texture = textureFactory.Create(targetImage, true);
                return new TextureAtlas<TTexture>(texture, regions);
            }
        }

        public void Dispose()
        {
            foreach (var image in images.Values) {
                image.Dispose();
            }

            images.Clear();
        }
    }
}
