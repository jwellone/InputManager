#nullable enable

namespace jwellone
{
    public interface IInputManager
    {
        bool isInput { get; }
        bool isFlick { get; }
        bool isFlickLeft { get; }
        bool isFlickUp { get; }
        bool isFlickRight { get; }
        bool isFlickDown { get; }
        bool isPinching { get; }
        bool isTwoFingerRotation { get; }
        int touchCount { get; }
        float twoFingerRotation { get; }
        float pinchingDelta { get; }
        float repeatFrame { get; }
        float doubleTapEnableFrame { get; }

        InputHandler Get(int index);
    }
}