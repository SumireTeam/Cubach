using Cubach.View.OpenGL;

namespace Cubach
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            using (var window = new GLWindow(width: 800, height: 600, title: "Cubach"))
            {
                window.Run();
            }
        }
    }
}
