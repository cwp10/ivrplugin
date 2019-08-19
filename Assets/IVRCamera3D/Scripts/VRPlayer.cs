using UnityEngine;

namespace IVR
{
    public class VRPlayer : MonoBehaviour, IRayPointer
    {
        private VRRayPointer _rayPointer = null;

        public GameObject SelectedObject
        {
            get; private set;
        }

        public float DistanceOfSelectedObject
        {
            get; private set;
        }

        private void Awake()
        {
            _rayPointer = GetComponent<VRRayPointer>();
        }

        private void OnEnable()
        {
            RayPointerEventManager.instance?.Register(this);
        }

        private void OnDisable()
        {
            RayPointerEventManager.instance?.UnRegister(this);
        }

        public void OnPointIn(RayPointEventArgs e)
        {
            if (e.target != null)
            {
                SelectedObject = e.target.gameObject;
                DistanceOfSelectedObject = e.distance;
                Debug.Log("OnPointerIn: " + SelectedObject.name + " Distance: " + e.distance);
            }
        }

        public void OnPointOut(RayPointEventArgs e)
        {
            if (e.target != null)
            {
                Debug.Log("OnPointerOut: " + SelectedObject.name);
                SelectedObject = null;
                DistanceOfSelectedObject = 0;
            }
        }
    }
}
