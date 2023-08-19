using System;
using UnityEngine;

#nullable enable

namespace jwellone
{
    [DefaultExecutionOrder(-999)]
    public abstract class InputManagerBase<T, THandler> : SingletonMonoBehaviour<T>, IInputManager where T : InputManagerBase<T, THandler> where THandler : InputHandler, new()
    {
        [Serializable]
        class EmptyInputHandler : InputHandler
        {
            public override Vector2 position => Vector2.zero;
            protected override bool _isBegan => false;
            protected override bool _isStationary => false;
            protected override bool _isMoved => false;
            protected override bool _isEnded => false;
            protected override bool _isCanceled => false;
            protected override int _pointerId => 0;
        }

        private enum FlickDir
        {
            None = 0,
            Left,
            Up,
            Right,
            Down
        }

        readonly InputHandler _emptyHandler = new EmptyInputHandler();

        [SerializeField] float _repeatFrame = 1.0f;
        [SerializeField] float _doubleTapEnableFrame = 0.2f;
        [SerializeField] float _flickEnableTime = 0.2f;
        [SerializeField] float _flickMinimumDistance = 150;
        [SerializeField] bool _pinchingWidthReference = true;
        [SerializeField] float _rotateEnabledMinValue = 0.1f;

        FlickDir _flickDir = FlickDir.None;
        float _prevPinchingDistance;
        Vector2 _prevPinchingCenter;
        Vector2 _prevRotatePos1;
        Vector2 _prevRotatePos2;
        THandler[] _handlers = null!;

        protected override bool isDontDestroyOnLoad => true;

        int _checkCount
        {
            get
            {
#if UNITY_EDITOR
                return _handlers.Length;
#else
				return touchCount;
#endif
            }
        }

        public virtual int createNum
        {
            get
            {
#if UNITY_EDITOR
                return 3;
#else
				return 5;
#endif
            }
        }

        public int touchCount
        {
            get
            {
#if UNITY_EDITOR
                int count = 0;
                for (int i = 0; i < _checkCount; ++i)
                {
                    var handler = Get(i);
                    if (handler.isTap || handler.isDown)
                    {
                        ++count;
                    }
                }
                return count;
#else
				return Math.Min(Input.touchCount, _handlers.Length);
#endif
            }
        }

        public bool isInput
        {
            get
            {
                for(var i = 0; i < _handlers.Length; ++i)
                {
                    if(_handlers[i].isInput)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool isFlick => FlickDir.None != _flickDir;
        public bool isFlickLeft => FlickDir.Left == _flickDir;
        public bool isFlickUp => FlickDir.Up == _flickDir;
        public bool isFlickRight => FlickDir.Right == _flickDir;
        public bool isFlickDown => FlickDir.Down == _flickDir;
        public bool isPinching { get; private set; }
        public bool isTwoFingerRotation { get; private set; }
        public float twoFingerRotation { get; private set; }
        public float pinchingDelta { get; private set; }

        public float repeatFrame
        {
            get => _repeatFrame;
            set => _repeatFrame = Mathf.Max(0f, value);
        }

        public float doubleTapEnableFrame
        {
            get => _doubleTapEnableFrame;
            set => _doubleTapEnableFrame = value;
        }

        protected override void OnAwakened()
        {
            _handlers = new THandler[createNum];
            for (int i = 0; i < _handlers.Length; ++i)
            {
                var hander = new THandler();
                hander.Initialization(this, i);
                _handlers[i] = hander;
            }

            _emptyHandler.Initialization(this, 0);
        }

        protected override void OnDestroyed()
        {
        }

        protected virtual void Update()
        {
            var unscaleDeltaTime = Time.unscaledDeltaTime;
            for (int i = 0; i < _handlers!.Length; ++i)
            {
                _handlers[i].Update(unscaleDeltaTime);
            }

            UpdateFlick();
            UpdatePinching();
            UpdateTwoFingerRotation();
        }

        protected virtual void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                OnReset();
            }
        }

        void UpdateFlick()
        {
            _flickDir = FlickDir.None;

            var hander = Get(0);
            if (!hander.isUp)
            {
                return;
            }

            var dir = hander.position - hander.startPosition;
            var dist = dir.magnitude;
            if (hander.inputDuration > _flickEnableTime || _flickMinimumDistance > dist)
            {
                return;
            }

            var deg = 180 + Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (45 > deg || 315 < deg)
            {
                _flickDir = FlickDir.Left;
            }
            else if (315 >= deg && 225 <= deg)
            {
                _flickDir = FlickDir.Up;
            }
            else if (225 > deg && 135 < deg)
            {
                _flickDir = FlickDir.Right;
            }
            else
            {
                _flickDir = FlickDir.Down;
            }
        }

        void UpdatePinching()
        {
            isPinching = false;

            if (touchCount != 2)
            {
                return;
            }

            var h0 = Get(0);
            var h1 = Get(1);

            if (h1.isTap)
            {
                var position0 = h0.position;
                var position1 = h1.position;
                _prevPinchingDistance = Vector2.Distance(position0, position1);
                _prevPinchingCenter = Vector2.Lerp(position0, position1, 0.5f);
            }
            else if (h0.isMoving || h1.isMoving)
            {
                var position0 = h0.position;
                var position1 = h1.position;
                var center = Vector2.Lerp(position0, position1, 0.5f);
                var dist = Vector2.Distance(position0, position1);
                pinchingDelta = (dist - _prevPinchingDistance) / (_pinchingWidthReference ? Screen.width : Screen.height);
                isPinching = Vector2.Distance(_prevPinchingCenter, center) < 10f && Mathf.Abs(_prevPinchingDistance = dist) > 10f;
                _prevPinchingDistance = dist;
                _prevPinchingCenter = center;
            }
        }

        void UpdateTwoFingerRotation()
        {
            isTwoFingerRotation = false;
            twoFingerRotation = 0.0f;

            if (touchCount != 2)
            {
                return;
            }

            var h0 = Get(0);
            var h1 = Get(1);

            if (h1.isTap)
            {
                _prevRotatePos1 = h0.position;
                _prevRotatePos2 = h1.position;
            }
            else if (h0.isMoving || h1.isMoving)
            {
                var pos1 = h0.position;
                var pos2 = h1.position;

                var prevDist = _prevRotatePos2 - _prevRotatePos1;
                var prevAngle = Mathf.Atan2(prevDist.y, prevDist.x) * Mathf.Rad2Deg;

                var nowDist = pos2 - pos1;
                var nowAngle = Mathf.Atan2(nowDist.y, nowDist.x) * Mathf.Rad2Deg;

                var angle = prevAngle - nowAngle;
                if (angle < -180) { angle += 360.0f; }
                else if (angle > 180.0f) { angle -= 360.0f; }

                if (_rotateEnabledMinValue > Mathf.Abs(angle))
                {
                    angle = 0.0f;
                }

                twoFingerRotation = angle;
                isTwoFingerRotation = angle != 0.0f;

                _prevRotatePos1 = pos1;
                _prevRotatePos2 = pos2;
            }
        }

        public void OnReset()
        {
            foreach (var handle in _handlers)
            {
                handle.Reset();
            }
        }

        public InputHandler Get(int index)
        {
            return (0 <= index && _checkCount > index) ? _handlers[index] : _emptyHandler;
        }

        public static bool multiTouchEnabled
        {
            get { return Input.multiTouchEnabled; }
            set { Input.multiTouchEnabled = value; }
        }

        public static Vector3 acceleration
        {
            get { return Input.acceleration; }
        }

        public static void ResetInputAxes()
        {
            Input.ResetInputAxes();
        }

        public static string compositionString
        {
            get { return Input.compositionString; }
        }

        public static DeviceOrientation deviceOrientation
        {
            get { return Input.deviceOrientation; }
        }

        public static string inputString
        {
            get { return Input.inputString; }
        }

        public static bool touchSupported
        {
            get { return Input.touchSupported; }
        }

        Vector3 mousePosition
        {
            get { return Input.mousePosition; }
        }

        static Touch GetTouch(int index)
        {
            return Input.GetTouch(index);
        }

        static bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        static bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        static bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }
    }
}
