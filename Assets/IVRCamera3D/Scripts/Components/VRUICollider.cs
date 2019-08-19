using UnityEngine;

namespace IVR
{
    [RequireComponent(typeof(RectTransform))]
    public class VRUICollider : MonoBehaviour
    {
        private BoxCollider _boxCollider = null;
        private RectTransform _rectTransform = null;

        private void OnEnable()
        {
            ValidateCollider();
        }

        private void OnValidate()
        {
            ValidateCollider();
        }

        private void ValidateCollider()
        {
            _rectTransform = GetComponent<RectTransform>();

            _boxCollider = GetComponent<BoxCollider>();

            if (_boxCollider == null)
            {
                _boxCollider = this.gameObject.AddComponent<BoxCollider>();
            }

            _boxCollider.size = _rectTransform.sizeDelta;
        }
    }
}