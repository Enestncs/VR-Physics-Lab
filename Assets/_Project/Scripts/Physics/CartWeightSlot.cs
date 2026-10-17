using UnityEngine;
using BNG;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Trigger zone on top of the physics cart that detects and registers placed weight blocks.
    /// </summary>
    public class CartWeightSlot : MonoBehaviour
    {
        [SerializeField] private CartPhysics cartPhysics;

        private void Awake()
        {
            if (cartPhysics == null)
            {
                cartPhysics = GetComponentInParent<CartPhysics>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb != null && rb != cartPhysics.Rigidbody)
            {
                cartPhysics.AddWeight(rb);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb != null && rb != cartPhysics.Rigidbody)
            {
                cartPhysics.RemoveWeight(rb);
            }
        }
    }
}
