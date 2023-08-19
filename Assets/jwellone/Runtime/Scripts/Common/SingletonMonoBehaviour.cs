using UnityEngine;

#nullable enable

namespace jwellone
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T? _instance;

        public static T? instance
        {
            get
            {
                if (!isExists)
                {
                    instance = FindObjectOfType<T>();
                    Debug.Assert(isExists, $"{typeof(T).Name} The instance does not exist.");
                }

                return _instance;
            }

            private set
            {
                _instance = value;
                isExists = _instance != null;
            }
        }

        public static bool isExists
        {
            get;
            private set;
        }

        protected virtual bool isDontDestroyOnLoad => false;

        void Awake()
        {
            if (!isExists)
            {
                instance = (T)this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnAwakened();
        }

        void OnDestroy()
        {
            OnDestroyed();
            if (_instance == this)
            {
                instance = null;
            }
        }

        protected abstract void OnAwakened();

        protected abstract void OnDestroyed();
    }
}