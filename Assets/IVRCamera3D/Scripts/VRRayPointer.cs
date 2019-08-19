using UnityEngine;

namespace IVR
{
    public struct RayPointEventArgs
    {
        public float distance;
        public UnityEngine.Transform target;
    }

    public class VRRayPointer : MonoBehaviour
    {
        public delegate void RayPointerEventHandler(object sender, RayPointEventArgs e);
        private Transform _previousTarget = null;

        private void Update()
        {
            UpdateRaycast();
        }

        private void UpdateRaycast()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            bool bHit = Physics.Raycast(ray, out hit);

            if (_previousTarget && _previousTarget != hit.transform)
            {
                RayPointEventArgs argsOut = new RayPointEventArgs();
                argsOut.distance = 0;
                argsOut.target = _previousTarget;
                RayPointerEventManager.instance?.PointOut(argsOut);
                _previousTarget = null;
            }

            if (bHit && _previousTarget != hit.transform)
            {
                RayPointEventArgs argsIn = new RayPointEventArgs();
                argsIn.distance = hit.distance;
                argsIn.target = hit.transform;
                RayPointerEventManager.instance?.PointIn(argsIn);
                _previousTarget = hit.transform;
            }

            if (!bHit)
            {
                _previousTarget = null;
            }
        }
    }
}