using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace Cubach.View
{
    public class FontPage<TTexture> : IDisposable where TTexture : ITexture
    {
        public readonly TTexture Texture;
        public readonly Dictionary<char, Vector2> CharSizes;

        public FontPage(TTexture texture, Dictionary<char, Vector2> sizes)
        {
            Texture = texture;
            CharSizes = sizes;
        }

        public void Dispose() => Texture.Dispose();
    }

    public class TextRenderer<TTexture> : IDisposable where TTexture : ITexture
    {
        private readonly ITextureFactory<TTexture> textureFactory;
        private readonly SpriteBatch<TTexture> spriteBatch;

        private readonly Dictionary<string, FontPage<TTexture>>
            fontPages = new Dictionary<string, FontPage<TTexture>>();

        private const int pageCols = 32;
        private const int pageRows = 32;

        public TextRenderer(ITextureFactory<TTexture> textureFactory, SpriteBatch<TTexture> spriteBatch)
        {
            this.textureFactory = textureFactory;
            this.spriteBatch = spriteBatch;
        }

        public Bitmap CreateFontPageBitmap(string fontFamily, int emSize, int page, out Dictionary<char, Vector2> sizes)
        {
            sizes = new Dictionary<char, Vector2>();

            int width = 4 * emSize * pageCols;
            int height = 4 * emSize * pageCols;

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap)) {
                Font font;

                var customFonts = Directory.GetDirectories("./Fonts");
                var customFont = customFonts.FirstOrDefault(f => f == $"./Fonts/{fontFamily}");
                if (customFont != null) {
                    using (var fontCollection = new PrivateFontCollection()) {
                        fontCollection.AddFontFile($"{customFont}/font.ttf");
                        font = new Font(fontCollection.Families[0], emSize);
                    }
                }
                else {
                    font = new Font(fontFamily, emSize, FontStyle.Bold);
                }

                try {
                    using (var brush = new SolidBrush(Color.White)) {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                        graphics.Clear(Color.Transparent);

                        SizeF s = graphics.MeasureString("||", font);

                        for (int i = 0; i < pageCols; ++i) {
                            for (int j = 0; j < pageRows; ++j) {
                                int x = emSize + 4 * i * emSize;
                                int y = emSize + 4 * j * emSize;

                                char ch = (char) (i + pageCols * j + pageCols * pageRows * page);
                                string str = ch.ToString();

                                graphics.DrawString(str, font, brush, x, y);
                                SizeF size = graphics.MeasureString($"|{str}|", font);
                                sizes.Add(ch, new Vector2(size.Width - s.Width, size.Height));
                            }
                        }
                    }
                }
                finally {
                    font.Dispose();
                }
            }

            return bitmap;
        }

        public FontPage<TTexture> CreateFontPage(string fontFamily, int emSize, int page)
        {
            using (var bitmap = CreateFontPageBitmap(fontFamily, emSize, page, out Dictionary<char, Vector2> sizes)) {
                var texture = textureFactory.Create(bitmap, true);
                return new FontPage<TTexture>(texture, sizes);
            }
        }

        public FontPage<TTexture> GetFontPage(string fontFamily, int emSize, int page)
        {
            string key = $"{fontFamily}:{emSize}:{page}";
            if (fontPages.ContainsKey(key)) {
                return fontPages[key];
            }

            var fontPage = CreateFontPage(fontFamily, emSize, page);
            fontPages[key] = fontPage;
            return fontPage;
        }

        public FontPage<TTexture> GetFontPage(string fontFamily, int emSize, char ch)
        {
            int page = ch / (pageCols * pageRows);
            return GetFontPage(fontFamily, emSize, page);
        }

        public (TTexture, TextureRegion) GetCharTextureRegion(string fontFamily, int emSize, char ch)
        {
            var fontPage = GetFontPage(fontFamily, emSize, ch);

            int j = ch % (pageCols * pageRows) / pageCols;
            int i = ch % (pageCols * pageRows) % pageCols;

            float uMin = 1f / pageCols * i;
            float vMin = 1f - 1f / pageRows * (j + 1);

            float uMax = uMin + 1f / pageCols;
            float vMax = vMin + 1f / pageRows;

            var region = new TextureRegion(new Vector2(uMin, vMin), new Vector2(uMax, vMax));
            return (fontPage.Texture, region);
        }

        public void DrawString(Vector2 position, string fontFamily, int emSize, string text, Color4 color)
        {
            spriteBatch.Begin();

            var chars = text.ToCharArray();
            var pos = position - new Vector2(emSize, emSize);
            var size = 4 * new Vector2(emSize, emSize);

            foreach (var ch in chars) {
                if (ch == '\n') {
                    pos.X = position.X - emSize;
                    pos.Y += 1.5f * emSize;
                }
                else {
                    FontPage<TTexture> page = GetFontPage(fontFamily, emSize, ch);
                    (TTexture texture, TextureRegion region) = GetCharTextureRegion(fontFamily, emSize, ch);
                    var sprite = new Sprite<TTexture>(pos, size, texture, region, color);
                    spriteBatch.Draw(sprite);
                    pos.X += page.CharSizes[ch].X;
                }
            }

            spriteBatch.End();
        }

        public Vector2 MeasureString(string fontFamily, int emSize, string text)
        {
            var chars = text.ToCharArray();
            var pos = Vector2.Zero;
            var size = Vector2.Zero;

            foreach (var ch in chars) {
                if (ch == '\n') {
                    pos.X = 0;
                    pos.Y += 1.5f * emSize;
                }
                else {
                    FontPage<TTexture> page = GetFontPage(fontFamily, emSize, ch);
                    pos.X += page.CharSizes[ch].X;

                    size.X = Math.Max(pos.X + page.CharSizes[ch].X, size.X);
                    size.Y = Math.Max(pos.Y + page.CharSizes[ch].Y, size.Y);
                }
            }

            return size;
        }

        public void Dispose()
        {
            foreach (var page in fontPages.Values) {
                page.Dispose();
            }

            fontPages.Clear();
        }
    }
}
