using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using GDIPixelFormat = System.Drawing.Imaging.PixelFormat;
using GLPixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Cubach.View.OpenGL
{
    public sealed class TextureHandle : GLObjectHandle
    {
        public TextureHandle(int handle) : base(handle) { }

        public static TextureHandle Create()
        {
            int handle = GL.GenTexture();
            return new TextureHandle(handle);
        }

        protected override void ReleaseHandle() => GL.DeleteTexture(Handle);
    }

    public class GLTexture : ITexture
    {
        public readonly TextureHandle Handle;

        public GLTexture() => Handle = TextureHandle.Create();

        public void Bind(int slot = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, (int)Handle);
        }

        public static void Bind(GLTexture texture, int slot = 0) => texture.Bind(slot);

        public void SetImage(Bitmap image, bool withAlpha = false)
        {
            Bind();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);

            int width = image.Width;
            int height = image.Height;

            // Normalize pixel format.
            var gdiPF = withAlpha ? GDIPixelFormat.Format32bppArgb : GDIPixelFormat.Format24bppRgb;
            using (Bitmap copy = new Bitmap(width, height, gdiPF))
            {
                using (Graphics graphics = Graphics.FromImage(copy))
                {
                    graphics.DrawImage(image, 0, 0);
                }

                copy.RotateFlip(RotateFlipType.RotateNoneFlipY);

                var rect = new Rectangle(0, 0, width, height);
                BitmapData data = copy.LockBits(rect, ImageLockMode.ReadOnly, gdiPF);

                var pif = withAlpha ? PixelInternalFormat.Rgba : PixelInternalFormat.Rgb;
                var glPF = withAlpha ? GLPixelFormat.Bgra : GLPixelFormat.Bgr;
                GL.TexImage2D(TextureTarget.Texture2D, 0, pif, width, height, 0, glPF, PixelType.UnsignedByte, data.Scan0);
                copy.UnlockBits(data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Dispose() => Handle.Dispose();
    }
}
