using System;
using UnityEngine;

#nullable enable

namespace jwellone
{
    [Serializable]
    public class EditorInputHandler : InputHandler
    {
        public override Vector2 position => Input.mousePosition;
        protected override bool _isBegan => Input.GetMouseButtonDown(index);
        protected override bool _isStationary => Input.GetMouseButton(index);
        protected override bool _isMoved => (_isStationary && (0 != Input.GetAxis("Mouse X") || 0 != Input.GetAxis("Mouse Y")));
        protected override bool _isEnded => Input.GetMouseButtonUp(index);
        protected override bool _isCanceled => false;
        protected override int _pointerId => 0;
    }
}