using UnityEngine;
using UnityEngine.Events;

namespace VRPhysicsLab.Measurement
{
    public enum GateType
    {
        StartGate,
        EndGate
    }

    /// <summary>
    /// Optical photogate sensor placed on the track to detect the exact instant the cart passes through.
    /// </summary>
    public class GateSensor : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GateType gateType = GateType.StartGate;

        [Header("Events")]
        public UnityEvent<GateSensor, Collider> onCartTriggered;

        public GateType Type => gateType;

        private void OnTriggerEnter(Collider other)
        {
            // Trigger on cart
            if (other.GetComponentInParent<VRPhysicsLab.Physics.CartPhysics>() != null)
            {
                onCartTriggered?.Invoke(this, other);
            }
        }
    }
}
