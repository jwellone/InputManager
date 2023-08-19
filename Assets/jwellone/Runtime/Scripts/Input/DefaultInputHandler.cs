using System;
using UnityEngine;

#nullable enable

namespace jwellone
{
    [Serializable]
    public class DefaultInputHandler : InputHandler
    {
        bool _isEnabled => Input.touchCount > index;
        TouchPhase _phase => Input.GetTouch(index).phase;

        public override Vector2 position => _isEnabled ? Input.GetTouch(index).position : Vector2.zero;
        protected override bool _isBegan => _isEnabled && (TouchPhase.Began == _phase);
        protected override bool _isStationary => _isEnabled && (TouchPhase.Stationary == _phase);
        protected override bool _isMoved => _isEnabled && (TouchPhase.Moved == _phase);
        protected override bool _isEnded => _isEnabled && (TouchPhase.Ended == _phase);
        protected override bool _isCanceled => _isEnabled && (TouchPhase.Canceled == _phase);
        protected override int _pointerId => _isEnabled ? Input.GetTouch(index).fingerId : -1;
    }
}