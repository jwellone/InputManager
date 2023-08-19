using System;
using UnityEngine;

#nullable enable

namespace jwellone
{
    [Serializable]
    public abstract class InputHandler
    {
        [Flags]
        public enum StatusFlags : uint
        {
            None = 0x0,
            Tap = 0x1 << 0,
            DoubleTap = 0x1 << 1,
            Down = 0x1 << 2,
            Up = 0x1 << 3,
            Repeat = 0x1 << 4,
            Moving = 0x1 << 5,
            Canceled = 0x1 << 6,
            EnableDoubleTap = 0x1 << 7,
            InputBit = (Tap | DoubleTap | Down | Moving),
            UpdateResetBit = (InputBit | Up | Repeat | Canceled),
        }

        int _index;
        StatusFlags _status;
        StatusFlags _prevStatus;
        IInputManager _owner = null!;

        public int index { get { return _index; } private set { _index = value; } }
        public bool isInput => HasStatusFlags(StatusFlags.InputBit);
        public bool isTap => HasStatusFlags(StatusFlags.Tap);
        public bool isPrevTap => HasStatusFlags(_prevStatus, StatusFlags.Tap);
        public bool isDoubleTap => HasStatusFlags(StatusFlags.DoubleTap);
        public bool isDown => HasStatusFlags(StatusFlags.Down);
        public bool isPrevDown => HasStatusFlags(_prevStatus, StatusFlags.Down);
        public bool isUp => HasStatusFlags(StatusFlags.Up);
        public bool isRepeat => HasStatusFlags(StatusFlags.Repeat);
        public bool isMoving => HasStatusFlags(StatusFlags.Moving);
        public float repeatFrame => _owner.repeatFrame;
        public float doubleTapEnableFrame => _owner.doubleTapEnableFrame;

        public float inputDuration { get; private set; }
        private float localFrame { get; set; }
        public virtual float inputValue { get { return isInput ? 1.0f : 0.0f; } }
        public Vector2 startPosition { get; private set; }

        public void Initialization(IInputManager owner, int index)
        {
            _owner = owner;
            _index = index;
            Reset();
        }

        public virtual void Update(float deltaTime)
        {
            if (_isCanceled)
            {
                _status = StatusFlags.Canceled;
                return;
            }

            _prevStatus = _status;
            ResetStatusFlags(StatusFlags.UpdateResetBit);

            if (_isBegan)
            {
                var flags = HasStatusFlags(StatusFlags.EnableDoubleTap) ? StatusFlags.DoubleTap : StatusFlags.Tap;
                AddStatusFlags(flags);
                localFrame = 0.0f;
                inputDuration = 0.0f;
                startPosition = position;
            }
            else if (_isMoved || _isStationary)
            {
                var flags = _isMoved ? StatusFlags.Down | StatusFlags.Moving : StatusFlags.Down;
                AddStatusFlags(flags);
            }
            else if (_isEnded)
            {
                AddStatusFlags(StatusFlags.Up);

                if (HasStatusFlags(StatusFlags.EnableDoubleTap) || inputDuration > doubleTapEnableFrame)
                {
                    ResetStatusFlags(StatusFlags.EnableDoubleTap);
                }
                else
                {
                    AddStatusFlags(StatusFlags.EnableDoubleTap);
                }

                localFrame = 0.0f;
                return;
            }
            else if (HasStatusFlags(StatusFlags.EnableDoubleTap))
            {
                localFrame += deltaTime;
                if (localFrame > doubleTapEnableFrame)
                {
                    Reset();
                }
                return;
            }
            else
            {
                Reset();
                return;
            }

            inputDuration += deltaTime;

            localFrame += deltaTime;
            if (repeatFrame <= localFrame)
            {
                while (localFrame >= repeatFrame)
                {
                    localFrame -= repeatFrame;
                }

                AddStatusFlags(StatusFlags.Repeat);
            }
        }

        public void Reset()
        {
            _status = StatusFlags.None;
            _prevStatus = StatusFlags.None;
            inputDuration = 0.0f;
            localFrame = 0.0f;
        }

        public bool IsInputDuration(float checkFrame)
        {
            return !isDown ? false : (inputDuration <= checkFrame);
        }

        void AddStatusFlags(StatusFlags flags)
        {
            _status |= flags;
        }

        bool HasStatusFlags(StatusFlags flags)
        {
            return HasStatusFlags(_status, flags);
        }

        bool HasStatusFlags(StatusFlags status, StatusFlags flags)
        {
            return (status & flags) != 0;
        }

        void ResetStatusFlags(StatusFlags flags)
        {
            _status = _status & ~flags;
        }

        protected abstract bool _isBegan { get; }
        protected abstract bool _isStationary { get; }
        protected abstract bool _isEnded { get; }
        protected abstract bool _isMoved { get; }
        protected abstract bool _isCanceled { get; }
        protected abstract int _pointerId { get; }
        public abstract Vector2 position { get; }
    }
}