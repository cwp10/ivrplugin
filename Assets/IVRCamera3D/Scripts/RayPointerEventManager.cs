using UnityEngine;
using System.Collections.Generic;

namespace IVR
{
    public class RayPointerEventManager : MonoBehaviour
    {
        private static bool _applicationIsQuitting = false;
        private static RayPointerEventManager _instance = null;
        public static RayPointerEventManager instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    return null;
                }
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RayPointerEventManager>();

                    if (_instance == null)
                    {
                        _instance = new GameObject(typeof(RayPointerEventManager).ToString()).AddComponent<RayPointerEventManager>();
                    }
                }
                return _instance;
            }
        }

        private List<IRayPointer> _rayPointers = new List<IRayPointer>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            _applicationIsQuitting = true;
        }

        public virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
            _instance = null;
        }

        public void Register(IRayPointer rayPointer)
        {
            if (!_rayPointers.Contains(rayPointer))
            {
                _rayPointers.Add(rayPointer);
            }
        }

        public void UnRegister(IRayPointer rayPointer)
        {
            if (_rayPointers.Contains(rayPointer))
            {
                _rayPointers.Remove(rayPointer);
            }
        }

        public void PointIn(RayPointEventArgs e)
        {
            foreach (IRayPointer pointer in _rayPointers)
            {
                pointer.OnPointIn(e);
            }
        }

        public void PointOut(RayPointEventArgs e)
        {
            foreach (IRayPointer pointer in _rayPointers)
            {
                pointer.OnPointOut(e);
            }
        }
    }
}