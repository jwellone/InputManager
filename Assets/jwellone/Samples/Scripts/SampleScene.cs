using System.Text;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace jwellone.Samples
{
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] Text _text = null!;
        [SerializeField] Transform _target = null!;

        InputManager _inputManager => InputManager.instance!;
        InputHandler.StatusFlags[]? _prevStatusFlagsArray;

        void Awake()
        {
            _text.text = string.Empty;
            _prevStatusFlagsArray = new InputHandler.StatusFlags[_inputManager.createNum];
        }

        void Update()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _inputManager.createNum; ++i)
            {
                AppendInputInfoIfNeed(i, sb);
            }

            var str = sb.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                _text.text += str;
            }

            if (!_inputManager.isPinching)
            {
                return;
            }
            var scale = _target.localScale;
            scale += Vector3.one * _inputManager.pinchingDelta * 5;
            _target.localScale = scale;

            _target.Rotate(new Vector3(0, 0, -_inputManager.twoFingerRotation));
        }

        public void OnClickReset()
        {
            _text.text = string.Empty;
        }

        void AppendInputInfoIfNeed(int index, StringBuilder sb)
        {
            var flags = InputHandler.StatusFlags.None;
            var h = _inputManager.Get(index);
            var text = string.Empty;

            if (h.isTap)
            {
                flags |= InputHandler.StatusFlags.Tap;
                text += "Tap|";
            }

            if (h.isDoubleTap)
            {
                flags |= InputHandler.StatusFlags.DoubleTap;
                text += "DoubleTap|";
                Debug.Log($"Double Tap");
            }

            if (h.isRepeat)
            {
                flags |= InputHandler.StatusFlags.Repeat;
                text += "Repeat|";
            }

            if (h.isDown)
            {
                flags |= InputHandler.StatusFlags.Down;
                text += "Down|";
            }

            if (h.isUp)
            {
                flags |= InputHandler.StatusFlags.Up;
                text += "Up|";
            }

            if (flags == _prevStatusFlagsArray![index])
            {
                return;
            }

            _prevStatusFlagsArray![index] = flags;

            if (!string.IsNullOrEmpty(text))
            {
                sb.Append("Input[").Append(index).Append("] ").Append(text).Append(" ").AppendLine(Time.frameCount.ToString());
            }
        }
    }
}