using UnityEngine;
using UnityEngine.Events;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Interactive button that triggers the force launcher / experiment start.
    /// Can be activated by VR hand touch, controller ray, or inspector button.
    /// </summary>
    public class PhysicalLauncherButton : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private ForceApplicator forceApplicator;

        [Header("Visual Feedback")]
        [SerializeField] private Transform buttonCap;
        [SerializeField] private Vector3 pressedOffset = new Vector3(0, -0.015f, 0);
        [SerializeField] private float returnSpeed = 5f;

        [Header("Events")]
        public UnityEvent onButtonPressed;

        private Vector3 initialCapPosition;
        private bool isPressed = false;

        private void Start()
        {
            if (buttonCap != null)
            {
                initialCapPosition = buttonCap.localPosition;
            }
        }

        private void Update()
        {
            if (buttonCap != null && !isPressed)
            {
                buttonCap.localPosition = Vector3.Lerp(buttonCap.localPosition, initialCapPosition, Time.deltaTime * returnSpeed);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Triggered by VR hand or controller
            if (!isPressed)
            {
                PressButton();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            isPressed = false;
        }

        [ContextMenu("Press Button")]
        public void PressButton()
        {
            isPressed = true;
            if (buttonCap != null)
            {
                buttonCap.localPosition = initialCapPosition + pressedOffset;
            }

            if (forceApplicator != null)
            {
                forceApplicator.StartApplyingForce();
            }

            onButtonPressed?.Invoke();
        }
    }
}
