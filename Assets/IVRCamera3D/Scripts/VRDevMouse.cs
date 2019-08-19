using UnityEngine;

namespace IVR
{
    public class VRDevMouse : MonoBehaviour
    {
        private const float kSensitivity = 1.0f;
        private Transform _bodyTransform = null;
        private float _mouseX = 0;
        private float _mouseY = 0;
        private float _mouseZ = 0;

        void Awake()
        {
            _bodyTransform = this.transform.parent;
            _bodyTransform.SetParent(this.transform.parent, false);

            this.transform.SetParent(_bodyTransform, false);
        }

        void Update()
        {
            if (IVRDevice.IsHMDAttached()) return;

            bool rolled = false;

#if UNITY_EDITOR_OSX
            if (Input.GetKey(KeyCode.Alpha3))
            {
                _bodyTransform.localRotation = Quaternion.identity;
                _mouseX = 0;
                _mouseY = 0;
            }
#endif

            if (Input.GetKey(KeyCode.LeftControl))
            {
                _mouseZ += Input.GetAxis("Mouse X") * kSensitivity;
                _mouseZ = Mathf.Clamp(_mouseZ, -85, 85);
            }
            else
            {
                _mouseX += Input.GetAxis("Mouse X") * kSensitivity;

                if (_mouseX <= -180) _mouseX += 360;
                else if (_mouseX > 180) _mouseX -= 360;

                _mouseY -= Input.GetAxis("Mouse Y") * kSensitivity;
                _mouseY = Mathf.Clamp(_mouseY, -85, 85);
            }

            _bodyTransform.localRotation = Quaternion.Euler(0, this.transform.localRotation.eulerAngles.y, 0.0f) * Quaternion.Euler(_mouseY, _mouseX, _mouseZ);
        }
    }
}

