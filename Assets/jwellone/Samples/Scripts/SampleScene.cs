using System.Collections;
using UnityEngine;

#nullable enable

namespace jwellone.Samples
{
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] float _duration = 0.25f;
        [SerializeField] Transform _target = null!;
        [SerializeField] GameObject _prefab = null!;
        [SerializeField] Material _mat1 = null!;
        [SerializeField] Material _mat2 = null!;
        InputManager? _inputManager => InputManager.instance;

        Coroutine? _coMove;

        void Update()
        {
            var handle = _inputManager!.Get(0);
            if (handle.isDoubleTap || handle.isRepeat)
            {
                var ray = Camera.main.ScreenPointToRay(handle.position);
                if (Physics.Raycast(ray, out var hit))
                {
                    var instance = Instantiate(_prefab);
                    instance.transform.position = hit.point + Vector3.up / 2f;
                    instance.GetComponent<MeshRenderer>().material = handle.isDoubleTap ? _mat1 : _mat2;
                    instance.SetActive(true);
                }
            }

            InputMove();
        }

        void InputMove()
        {
            if (_coMove != null)
            {
                return;
            }

            if (_inputManager!.isFlickLeft)
            {
                _coMove = StartCoroutine(Move(_target.position + Vector3.left, Quaternion.AngleAxis(90, Vector3.forward) * _target.rotation));
            }
            else if (_inputManager!.isFlickRight)
            {
                _coMove = StartCoroutine(Move(_target.position + Vector3.right, Quaternion.AngleAxis(-90, Vector3.forward) * _target.rotation));
            }
            else if (_inputManager!.isFlickUp)
            {
                _coMove = StartCoroutine(Move(_target.position + Vector3.forward, Quaternion.AngleAxis(90, Vector3.right) * _target.rotation));
            }
            else if (_inputManager!.isFlickDown)
            {
                _coMove = StartCoroutine(Move(_target.position + Vector3.back, Quaternion.AngleAxis(-90, Vector3.right) * _target.rotation));
            }
        }

        IEnumerator Move(Vector3 targetPos, Quaternion targetRot)
        {
            var time = 0f;
            var pos = _target.position;
            var rot = _target.rotation;
            var rate = 0f;
            var duration = Mathf.Max(0.01f, _duration);
            do
            {
                time += Time.deltaTime;
                rate = time / duration;
                _target.position = Vector3.Lerp(pos, targetPos, rate);
                _target.rotation = Quaternion.Lerp(rot, targetRot, rate);
                yield return null;
            } while (rate < 1f);

            _coMove = null;
        }
    }
}