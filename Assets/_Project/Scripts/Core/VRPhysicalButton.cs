using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using BNG;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Robust, satisfying physical VR button designed for Meta Quest 3 and BNG Framework.
    /// Supports hand touch/poke, palm presses, laser pointer clicks, and editor keyboard shortcuts.
    /// Provides mechanical spring animation, audio click feedback, and controller haptic rumble.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class VRPhysicalButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        [Header("Plunger Setup")]
        [Tooltip("The moving part of the button that depresses.")]
        [SerializeField] private Transform plunger;
        [Tooltip("Maximum travel distance downwards in meters.")]
        [SerializeField] private float travelDistance = 0.018f;
        [Tooltip("Speed of spring return.")]
        [SerializeField] private float springSpeed = 18f;

        [Header("Feedback")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip releaseSound;
        [Tooltip("Haptic rumble amplitude (0 to 1).")]
        [Range(0f, 1f)]
        [SerializeField] private float hapticAmplitude = 0.75f;
        [Tooltip("Haptic rumble duration in seconds.")]
        [SerializeField] private float hapticDuration = 0.08f;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer plungerRenderer;
        [ColorUsage(true, true)]
        [SerializeField] private Color normalEmission = Color.black;
        [ColorUsage(true, true)]
        [SerializeField] private Color pressedEmission = new Color(0.4f, 0.4f, 0.4f);

        [Header("Editor & Shortcut")]
        [SerializeField] private KeyCode keyboardShortcut = KeyCode.None;

        [Header("Events")]
        public UnityEvent onPressed;
        public UnityEvent onReleased;

        private Vector3 initialLocalPos;
        private Vector3 pressedLocalPos;
        private AudioSource audioSource;
        private Material plungerMatInstance;
        private bool isPressed = false;
        private float lastPressTime = -1f;
        private const float Cooldown = 0.25f;

        // Tracks colliding objects to maintain depression while touching
        private readonly HashSet<Collider> touchingColliders = new HashSet<Collider>();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound

            if (plunger == null)
            {
                plunger = transform.Find("Plunger");
                if (plunger == null) plunger = transform;
            }

            initialLocalPos = plunger.localPosition;
            pressedLocalPos = initialLocalPos - new Vector3(0f, travelDistance, 0f);

            if (plungerRenderer != null)
            {
                plungerMatInstance = plungerRenderer.material;
                SetEmission(normalEmission);
            }
        }

        private void Update()
        {
            // Keyboard shortcut support for quick testing
            if (keyboardShortcut != KeyCode.None && Input.GetKeyDown(keyboardShortcut))
            {
                TriggerPress(ControllerHand.Right);
            }

            // Clean up any null / destroyed colliders
            touchingColliders.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

            // Determine target position based on whether anything is pressing it
            bool shouldBePressed = touchingColliders.Count > 0;
            Vector3 targetPos = shouldBePressed ? pressedLocalPos : initialLocalPos;

            // Smooth spring lerp
            plunger.localPosition = Vector3.Lerp(plunger.localPosition, targetPos, Time.deltaTime * springSpeed);

            // Handle press activation threshold
            float currentY = plunger.localPosition.y;
            float thresholdY = initialLocalPos.y - (travelDistance * 0.65f);

            if (currentY <= thresholdY && !isPressed)
            {
                ActivatePress();
            }
            else if (currentY > thresholdY && isPressed && !shouldBePressed)
            {
                ActivateRelease();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsValidPresser(other))
            {
                touchingColliders.Add(other);
                
                // Identify hand for haptics
                ControllerHand hand = GetControllerHand(other);
                TriggerHaptics(hand);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (touchingColliders.Contains(other))
            {
                touchingColliders.Remove(other);
            }
        }

        private bool IsValidPresser(Collider col)
        {
            if (col == null) return false;
            if (col.transform.IsChildOf(transform)) return false; // Ignore own colliders

            // Accept any VR hand, controller, or grabber
            if (col.GetComponentInParent<Grabber>() != null) return true;
            if (col.GetComponentInParent<UITrigger>() != null) return true;
            if (col.GetComponentInParent<HandController>() != null) return true;
            if (col.GetComponentInParent<BNGPlayerController>() != null) return true;

            // Also accept any object with a Rigidbody (e.g. tools, weights poked into button)
            if (col.attachedRigidbody != null) return true;

            // Accept character controller or tagged player
            if (col.CompareTag("Player")) return true;

            return false;
        }

        private ControllerHand GetControllerHand(Collider col)
        {
            string lowerName = col.transform.root.name.ToLower() + col.gameObject.name.ToLower();
            if (col.transform.parent != null) lowerName += col.transform.parent.name.ToLower();

            if (lowerName.Contains("left")) return ControllerHand.Left;
            return ControllerHand.Right;
        }

        private void ActivatePress()
        {
            if (Time.time - lastPressTime < Cooldown) return;
            lastPressTime = Time.time;
            isPressed = true;

            if (audioSource != null && clickSound != null)
            {
                audioSource.pitch = Random.Range(0.97f, 1.03f);
                audioSource.PlayOneShot(clickSound, 0.9f);
            }

            SetEmission(pressedEmission);
            onPressed?.Invoke();
        }

        private void ActivateRelease()
        {
            isPressed = false;
            SetEmission(normalEmission);

            if (audioSource != null && releaseSound != null)
            {
                audioSource.pitch = Random.Range(0.97f, 1.03f);
                audioSource.PlayOneShot(releaseSound, 0.7f);
            }

            onReleased?.Invoke();
        }

        public void TriggerPress(ControllerHand hand = ControllerHand.Right)
        {
            if (Time.time - lastPressTime < Cooldown) return;
            TriggerHaptics(hand);
            ActivatePress();

            if (Application.isPlaying)
            {
                StartCoroutine(PerformSimulatedRelease());
            }
            else
            {
                ActivateRelease();
            }
        }

        private System.Collections.IEnumerator PerformSimulatedRelease()
        {
            plunger.localPosition = pressedLocalPos;
            yield return new WaitForSeconds(0.12f);
            ActivateRelease();
        }

        private void TriggerHaptics(ControllerHand hand)
        {
            if (InputBridge.Instance != null)
            {
                InputBridge.Instance.VibrateController(50f, hapticAmplitude, hapticDuration, hand);
            }
        }

        private void SetEmission(Color color)
        {
            if (plungerMatInstance != null && plungerMatInstance.HasProperty("_EmissionColor"))
            {
                plungerMatInstance.SetColor("_EmissionColor", color);
                DynamicGI.SetEmissive(plungerRenderer, color);
            }
        }

        // Pointer click integration
        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerPress(ControllerHand.Right);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Optional pointer down feedback
        }

        private void OnDestroy()
        {
            if (plungerMatInstance != null)
            {
                Destroy(plungerMatInstance);
            }
        }
    }
}
