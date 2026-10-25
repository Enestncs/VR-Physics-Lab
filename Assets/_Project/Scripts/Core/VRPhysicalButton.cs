using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BNG;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Robust, satisfying physical VR button designed for Meta Quest 3 and BNG Framework.
    /// Supports hand touch/poke, palm presses, laser pointer clicks, and editor keyboard shortcuts.
    /// Provides mechanical spring animation, audio click feedback, and controller haptic rumble.
    ///
    /// Near interaction: a static touch zone (derived from the plunger's BoxCollider at rest) is polled
    /// every frame with Physics.OverlapBox, so it does not depend on OnTriggerEnter messages
    /// (which never reached this script when the trigger collider lived on a child without a Rigidbody)
    /// and does not flicker when the plunger moves down out of the finger.
    /// Far interaction: handled by <see cref="VRButtonRayInteractor"/> (auto-spawned), which calls
    /// <see cref="RemotePress"/> / <see cref="AddHover"/> / <see cref="RemoveHover"/>.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(AudioSource))]
    public class VRPhysicalButton : MonoBehaviour
    {
        [Header("Plunger Setup")]
        [Tooltip("The moving part of the button that depresses.")]
        [SerializeField] private Transform plunger;
        [Tooltip("Resting local position of the plunger when unpressed.")]
        [SerializeField] private Vector3 restingLocalPos = new Vector3(0f, 0.035f, 0f);
        [Tooltip("Maximum travel distance downwards in meters.")]
        [SerializeField] private float travelDistance = 0.015f;
        [Tooltip("Speed of spring return.")]
        [SerializeField] private float springSpeed = 22f;

        [Header("Touch Detection")]
        [Tooltip("Extra margin (meters) added around the plunger's collider when detecting fingers.")]
        [SerializeField] private float touchPadding = 0.015f;
        [Tooltip("How far (meters) above the cap the palm/grab point of an open hand may be and still press.")]
        [SerializeField] private float palmReach = 0.06f;
        [Tooltip("Layers checked for hand/finger colliders.")]
        [SerializeField] private LayerMask touchLayers = ~0;

        [Header("Feedback")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip releaseSound;
        [Tooltip("Haptic rumble amplitude (0 to 1).")]
        [Range(0f, 1f)]
        [SerializeField] private float hapticAmplitude = 0.85f;
        [Tooltip("Haptic rumble duration in seconds.")]
        [SerializeField] private float hapticDuration = 0.10f;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer plungerRenderer;
        [ColorUsage(true, true)]
        [SerializeField] private Color normalEmission = Color.black;
        [ColorUsage(true, true)]
        [SerializeField] private Color pressedEmission = new Color(0.85f, 0.65f, 0.05f);

        [Header("Editor & Shortcut")]
        [SerializeField] private KeyCode keyboardShortcut = KeyCode.None;

        [Header("Events")]
        public UnityEvent onPressed;
        public UnityEvent onReleased;

        private Vector3 pressedLocalPos;
        private AudioSource audioSource;
        private Material plungerMatInstance;
        private bool isPressed = false;
        private float lastPressTime = -10f;
        private const float Cooldown = 0.25f;
        private const float RemoteHoldTime = 0.15f;

        // Touch zone in this transform's local space
        private Vector3 zoneLocalCenter;
        private Vector3 zoneLocalHalfExtents;
        private readonly Collider[] overlapBuffer = new Collider[32];

        private float remoteHoldUntil = -1f;
        private int hoverCount = 0;
        private ControllerHand lastPressingHand = ControllerHand.Right;

        public bool IsPressed => isPressed;
        public bool IsHovered => hoverCount > 0;

        public Vector3 RestingLocalPos
        {
            get => restingLocalPos;
            set
            {
                restingLocalPos = value;
                UpdatePositions();
                CacheTouchZone();
            }
        }

        private void OnValidate()
        {
            if (plunger == null)
            {
                plunger = transform.Find("Plunger");
            }

            UpdatePositions();

            if (!Application.isPlaying && plunger != null)
            {
                plunger.localPosition = restingLocalPos;
            }
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1.0f; // 3D sound
            }

            if (plunger == null)
            {
                plunger = transform.Find("Plunger");
                if (plunger == null) plunger = transform;
            }

            UpdatePositions();

            if (Application.isPlaying)
            {
                if (plungerRenderer != null)
                {
                    plungerMatInstance = plungerRenderer.material;
                    plungerMatInstance.EnableKeyword("_EMISSION");
                    SetEmission(normalEmission);
                }

                // Make sure far (laser) interaction is available in every scene that has a button
                VRButtonRayInteractor.EnsureExists();
            }
        }

        private void Start()
        {
            UpdatePositions();
            if (plunger != null)
            {
                plunger.localPosition = restingLocalPos;
            }
            CacheTouchZone();
        }

        private void UpdatePositions()
        {
            if (restingLocalPos == Vector3.zero && plunger != null && plunger != transform)
            {
                restingLocalPos = plunger.localPosition;
            }
            pressedLocalPos = restingLocalPos - new Vector3(0f, travelDistance, 0f);
        }

        /// <summary>
        /// Computes a static touch zone from the plunger's BoxCollider in its resting pose.
        /// </summary>
        private void CacheTouchZone()
        {
            BoxCollider box = (plunger != null && plunger != transform) ? plunger.GetComponent<BoxCollider>() : null;
            if (box != null)
            {
                Vector3 centerInParent = restingLocalPos + plunger.localRotation * Vector3.Scale(box.center, plunger.localScale);
                Transform parent = plunger.parent;
                zoneLocalCenter = (parent == transform || parent == null)
                    ? centerInParent
                    : transform.InverseTransformPoint(parent.TransformPoint(centerInParent));
                zoneLocalHalfExtents = Vector3.Scale(box.size, plunger.localScale) * 0.5f;
            }
            else
            {
                zoneLocalCenter = restingLocalPos;
                zoneLocalHalfExtents = new Vector3(0.04f, 0.02f, 0.04f);
            }

            zoneLocalHalfExtents += Vector3.one * touchPadding;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (plunger != null)
                {
                    plunger.localPosition = restingLocalPos;
                }
                return;
            }

            // Keyboard shortcut support for quick testing
            if (keyboardShortcut != KeyCode.None && Input.GetKeyDown(keyboardShortcut))
            {
                RemotePress(ControllerHand.Right);
            }

            bool nearPressed = DetectNearPress(out ControllerHand nearHand);
            if (nearPressed) lastPressingHand = nearHand;

            bool remoteHeld = Time.time < remoteHoldUntil;
            bool wantPressed = nearPressed || remoteHeld;

            // Smooth spring lerp
            Vector3 targetPos = wantPressed ? pressedLocalPos : restingLocalPos;
            plunger.localPosition = Vector3.Lerp(plunger.localPosition, targetPos, Time.deltaTime * springSpeed);

            if (wantPressed && !isPressed)
            {
                ActivatePress(lastPressingHand);
            }
            else if (!wantPressed && isPressed)
            {
                ActivateRelease();
            }

            UpdateEmission();
        }

        /// <summary>
        /// Returns true if any player hand / finger is inside the touch zone.
        /// </summary>
        private bool DetectNearPress(out ControllerHand hand)
        {
            hand = ControllerHand.Right;

            Vector3 lossy = transform.lossyScale;
            Vector3 worldHalf = new Vector3(
                Mathf.Abs(zoneLocalHalfExtents.x * lossy.x),
                Mathf.Abs(zoneLocalHalfExtents.y * lossy.y),
                Mathf.Abs(zoneLocalHalfExtents.z * lossy.z));
            Vector3 worldCenter = transform.TransformPoint(zoneLocalCenter);

            int count = UnityEngine.Physics.OverlapBoxNonAlloc(worldCenter, worldHalf, overlapBuffer, transform.rotation,
                touchLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                if (IsValidPresser(overlapBuffer[i], out hand))
                {
                    return true;
                }
            }

            // Fallback for an open hand (BNG only enables finger colliders while pointing / making a fist):
            // accept the Grabber's palm point if it is right on top of the cap.
            var grabbers = VRButtonRayInteractor.Grabbers;
            if (grabbers != null)
            {
                for (int i = 0; i < grabbers.Count; i++)
                {
                    Grabber g = grabbers[i];
                    if (g == null || !g.isActiveAndEnabled) continue;

                    Vector3 local = transform.InverseTransformPoint(g.transform.position) - zoneLocalCenter;
                    if (Mathf.Abs(local.x) <= zoneLocalHalfExtents.x &&
                        Mathf.Abs(local.z) <= zoneLocalHalfExtents.z &&
                        local.y >= -zoneLocalHalfExtents.y &&
                        local.y <= zoneLocalHalfExtents.y + palmReach / Mathf.Max(0.0001f, Mathf.Abs(lossy.y)))
                    {
                        hand = g.HandSide;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidPresser(Collider col, out ControllerHand hand)
        {
            hand = ControllerHand.Right;
            if (col == null || !col.enabled) return false;
            if (col.transform.IsChildOf(transform)) return false; // Ignore own colliders
            if (col is CharacterController) return false;          // Ignore the player's body capsule

            // Only accept player hands / fingers.
            // This prevents loose objects (like weights) from accidentally pressing or holding the button down.
            UITrigger uiTrigger = col.GetComponentInParent<UITrigger>();
            Grabber grabber = col.GetComponentInParent<Grabber>();
            HandController handController = col.GetComponentInParent<HandController>();
            HandCollision handCollision = col.GetComponentInParent<HandCollision>();

            bool isHand = uiTrigger != null || grabber != null || handController != null || handCollision != null ||
                          col.name.Contains("UI Trigger") || col.name.Contains("index") || col.CompareTag("Player");
            if (!isHand) return false;

            // Large grab trigger spheres would press the button from far away; those are handled by the
            // palm-point fallback instead. Finger-tip UI triggers are fine.
            bool isFingerTip = uiTrigger != null || col.name.Contains("UI Trigger") || col.name.Contains("index3");
            if (col.isTrigger && !isFingerTip) return false;

            if (grabber != null) hand = grabber.HandSide;
            else if (handController != null && handController.grabber != null) hand = handController.grabber.HandSide;
            else if (handCollision != null && handCollision.HandGrabber != null) hand = handCollision.HandGrabber.HandSide;
            else hand = GuessHandFromName(col);

            return true;
        }

        private static ControllerHand GuessHandFromName(Collider col)
        {
            string lowerName = col.transform.root.name.ToLower() + col.gameObject.name.ToLower();
            if (col.transform.parent != null) lowerName += col.transform.parent.name.ToLower();
            return lowerName.Contains("left") ? ControllerHand.Left : ControllerHand.Right;
        }

        private bool ActivatePress(ControllerHand hand)
        {
            if (Time.time - lastPressTime < Cooldown) return false;
            lastPressTime = Time.time;
            isPressed = true;

            TriggerHaptics(hand);

            if (audioSource != null && clickSound != null)
            {
                audioSource.pitch = Random.Range(0.97f, 1.03f);
                audioSource.PlayOneShot(clickSound, 0.9f);
            }

            onPressed?.Invoke();
            return true;
        }

        private void ActivateRelease()
        {
            isPressed = false;

            if (audioSource != null && releaseSound != null)
            {
                audioSource.pitch = Random.Range(0.97f, 1.03f);
                audioSource.PlayOneShot(releaseSound, 0.7f);
            }

            onReleased?.Invoke();
        }

        /// <summary>
        /// Presses the button from a distance (laser pointer) or from code / keyboard.
        /// </summary>
        public void RemotePress(ControllerHand hand = ControllerHand.Right)
        {
            if (!Application.isPlaying)
            {
                onPressed?.Invoke();
                onReleased?.Invoke();
                return;
            }

            if (Time.time - lastPressTime < Cooldown) return;

            lastPressingHand = hand;
            remoteHoldUntil = Time.time + RemoteHoldTime;
            if (!isPressed)
            {
                ActivatePress(hand);
            }
        }

        /// <summary>
        /// Kept for backwards compatibility with existing UnityEvent bindings.
        /// </summary>
        public void TriggerPress(ControllerHand hand = ControllerHand.Right)
        {
            RemotePress(hand);
        }

        public void AddHover()
        {
            hoverCount++;
        }

        public void RemoveHover()
        {
            hoverCount = Mathf.Max(0, hoverCount - 1);
        }

        private void TriggerHaptics(ControllerHand hand)
        {
            if (InputBridge.Instance != null)
            {
                InputBridge.Instance.VibrateController(50f, hapticAmplitude, hapticDuration, hand);
            }
        }

        private void UpdateEmission()
        {
            if (plungerMatInstance == null) return;

            Color target = isPressed ? pressedEmission
                : (hoverCount > 0 ? Color.Lerp(normalEmission, pressedEmission, 0.45f) : normalEmission);
            SetEmission(target);
        }

        private Color currentEmission = new Color(-1f, -1f, -1f, -1f);

        private void SetEmission(Color color)
        {
            if (color == currentEmission) return;
            currentEmission = color;

            if (plungerMatInstance != null && plungerMatInstance.HasProperty("_EmissionColor"))
            {
                plungerMatInstance.SetColor("_EmissionColor", color);
                DynamicGI.SetEmissive(plungerRenderer, color);
            }
        }

        private void OnDisable()
        {
            hoverCount = 0;
        }

        private void OnDestroy()
        {
            if (plungerMatInstance != null)
            {
                Destroy(plungerMatInstance);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) CacheTouchZone();
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(zoneLocalCenter, zoneLocalHalfExtents * 2f);
        }
#endif
    }
}
