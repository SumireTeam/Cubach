namespace Cubach.View
{
    public interface ISpriteBatch<TTexture> where TTexture : ITexture
    {
        void Begin();
        void Draw(Sprite<TTexture> sprite);
        void End();
    }
}
