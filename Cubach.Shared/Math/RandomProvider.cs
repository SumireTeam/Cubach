namespace Cubach.Shared.Math
{
    public interface IRandomProvider
    {
        int Random(int x, int y = 0, int z = 0);
    }

    public class RandomProvider : IRandomProvider
    {
        private const long d = 9901;
        private const long sp = 333667;
        private const long xp = 99990001;
        private const long yp = 999999000001;
        private const long zp = 9999999900000001;

        public long Seed { get; private set; }

        public RandomProvider(long seed) => Seed = seed * sp;

        public int Random(int x, int y = 0, int z = 0) => (int)((x * xp ^ y * yp ^ z * zp ^ Seed) / d);
    }
}
