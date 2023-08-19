#nullable enable

namespace jwellone
{
#if UNITY_EDITOR
    using InputHandlerClass = EditorInputHandler;
#else
    using InputHandlerClass = DefaultInputHandler;
#endif

    public class InputManager : InputManagerBase<InputManager, InputHandlerClass>
    {
    }
}