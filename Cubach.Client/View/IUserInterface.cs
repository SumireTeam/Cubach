using OpenTK;
using OpenTK.Input;

namespace Cubach.View
{
    public interface IUserInterface
    {
        void Resize(uint width, uint height);

        void Update(INativeWindow window, float elapsedTime);
        bool HandleMouseInput(INativeWindow window, MouseState MouseState);
        bool HandleKeyboardInput(INativeWindow window, KeyboardState keyboardState);
        void PressChar(char keyChar);

        void Draw(float elapsedTime);
    }
}
