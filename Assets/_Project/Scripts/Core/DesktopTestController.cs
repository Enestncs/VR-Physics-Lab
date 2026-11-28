using UnityEngine;
using UnityEngine.XR;
using BNG;
using VRPhysicsLab.Physics;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Non-intrusive Desktop (PC Mouse + Keyboard) test controller for VR Physics Lab.
    /// Automatically disables itself whenever an XR/VR headset (e.g. Meta Quest 3 or Quest Link) is active,
    /// leaving the BNG XR Rig and headset integration 100% untouched.
    /// </summary>
    public class DesktopTestController : MonoBehaviour
    {
        [Header("Activation")]
        [Tooltip("Enable PC keyboard & mouse testing when no VR headset is active.")]
        [SerializeField] private bool enableDesktopModeWhenNoHMD = true;

        [Header("Movement & Look")]
        [SerializeField] private float walkSpeed = 2.4f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float minPitch = -75f;
        [SerializeField] private float maxPitch = 75f;

        [Header("Interaction")]
        [SerializeField] private float interactRange = 3.2f;
        [SerializeField] private float holdDistance = 0.72f;
        [SerializeField] private float minHoldDistance = 0.40f;
        [SerializeField] private float maxHoldDistance = 1.35f;
        [SerializeField] private float holdSmoothSpeed = 18f;

        [Header("Station Quick-Teleport Points (Facing +Z)")]
        [SerializeField] private Vector3 station1StandPos = new Vector3(0.0f, 0.50f, -0.20f);
        [SerializeField] private Vector3 station2StandPos = new Vector3(2.4f, 0.50f, -0.20f);
        [SerializeField] private Vector3 station3StandPos = new Vector3(-2.4f, 0.50f, -0.20f);

        private BNGPlayerController playerController;
        private CharacterController characterController;
        private Transform cameraRigTransform;
        private Camera mainCam;

        private bool isDesktopActive = false;
        private bool cursorLocked = false;
        private bool showHelpHud = true;
        private float yaw = 0f;
        private float pitch = 12f;

        // Held Grabbable state
        private Grabbable heldGrabbable;
        private Rigidbody heldRb;
        private bool heldWasKinematic;
        private bool heldUsedGravity;
        private Collider[] heldColliders;
        private Transform heldOriginalParent;

        // Current hover target
        private VRPhysicalButton hoveredButton;
        private Grabbable hoveredGrabbable;
        private CartPhysics hoveredCart;
        private SurfacePlateSlot hoveredSurfaceSlot;
        private string currentPrompt = "";

        private GUIStyle crosshairStyle;
        private GUIStyle promptStyle;
        private GUIStyle hudStyle;

        public bool IsDesktopActive => isDesktopActive;

        private static bool IsVRHeadsetActive()
        {
            if (XRSettings.isDeviceActive) return true;
            if (InputBridge.Instance != null && InputBridge.Instance.HMDActive) return true;
            return false;
        }

        private void Start()
        {
            if (!enableDesktopModeWhenNoHMD || IsVRHeadsetActive())
            {
                isDesktopActive = false;
                enabled = false;
                return;
            }

            playerController = Object.FindFirstObjectByType<BNGPlayerController>();
            if (playerController != null)
            {
                characterController = playerController.GetComponent<CharacterController>();
                cameraRigTransform = playerController.transform.Find("CameraRig");
            }
            mainCam = Camera.main;

            if (playerController == null || mainCam == null)
            {
                Debug.LogWarning("[DesktopTestController] PlayerController or Main Camera not found. Disabling desktop mode.");
                enabled = false;
                return;
            }

            // Initialize yaw/pitch from current view
            yaw = playerController.transform.eulerAngles.y;
            pitch = 10f;
            isDesktopActive = true;
            ApplyLookRotation();
            Debug.Log("[DesktopTestController] PC Test Mode active (no HMD detected). Click Game View or press Tab to lock cursor.");
        }

        private void Update()
        {
            // Safety guard: if a VR headset connects mid-session, hand control back immediately
            if (IsVRHeadsetActive())
            {
                ReleaseHeldObject();
                SetCursorLocked(false);
                isDesktopActive = false;
                enabled = false;
                return;
            }

            if (!isDesktopActive) return;

            bool lockedThisFrame = HandleCursorToggle();
            HandleQuickTeleports();

            if (Input.GetKeyDown(KeyCode.H))
            {
                showHelpHud = !showHelpHud;
            }

            if (cursorLocked)
            {
                HandleMouseLook();
                HandleMovement();
            }

            UpdateInteractionRaycast();
            if (!lockedThisFrame)
            {
                HandleInteractionInput();
            }
            UpdateHeldObject();
        }

        private bool HandleCursorToggle()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLocked(false);
                return false;
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                SetCursorLocked(!cursorLocked);
                return true;
            }
            else if (!cursorLocked && Input.GetMouseButtonDown(0))
            {
                // Lock cursor when clicking inside the Game View without firing an accidental interaction
                SetCursorLocked(true);
                return true;
            }
            return false;
        }

        private void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void HandleMouseLook()
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw += mx;
            pitch = Mathf.Clamp(pitch - my, minPitch, maxPitch);
            ApplyLookRotation();
        }

        private void ApplyLookRotation()
        {
            if (playerController != null)
            {
                playerController.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            if (cameraRigTransform != null)
            {
                cameraRigTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
            else if (mainCam != null)
            {
                mainCam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            if (characterController == null || !characterController.enabled) return;

            float h = 0f;
            float v = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

            Vector3 forward = playerController.transform.forward;
            Vector3 right = playerController.transform.right;
            Vector3 moveDir = (forward * v + right * h);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            float speed = walkSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
            Vector3 velocity = moveDir * speed;
            // Gentle downward gravity to keep CharacterController grounded
            velocity.y = characterController.isGrounded ? -0.5f : -9.81f;

            characterController.Move(velocity * Time.deltaTime);
        }

        private void HandleQuickTeleports()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) TeleportTo(station1StandPos);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) TeleportTo(station2StandPos);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) TeleportTo(station3StandPos);
        }

        private void TeleportTo(Vector3 targetPos)
        {
            if (playerController == null) return;

            if (characterController != null) characterController.enabled = false;
            playerController.transform.position = targetPos;
            yaw = 0f;
            pitch = 14f;
            ApplyLookRotation();
            if (characterController != null) characterController.enabled = true;
        }

        private void UpdateInteractionRaycast()
        {
            if (hoveredButton != null)
            {
                hoveredButton.RemoveHover();
                hoveredButton = null;
            }
            hoveredGrabbable = null;
            hoveredCart = null;
            hoveredSurfaceSlot = null;
            currentPrompt = "";

            if (mainCam == null) return;

            Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
            RaycastHit[] hits = UnityEngine.Physics.RaycastAll(ray, interactRange, ~0, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                // Ignore player body & currently held object
                if (hit.collider.GetComponentInParent<BNGPlayerController>() != null) continue;
                if (heldGrabbable != null && hit.collider.transform.IsChildOf(heldGrabbable.transform)) continue;

                // 1. If holding an object, check if aiming at a valid drop target (Cart or Surface Slot)
                if (heldGrabbable != null)
                {
                    var sp = heldGrabbable.GetComponent<SurfacePlate>();
                    if (sp != null)
                    {
                        var slot = hit.collider.GetComponentInParent<SurfacePlateSlot>();
                        if (slot != null)
                        {
                            hoveredSurfaceSlot = slot;
                            currentPrompt = $"[Sol Tık / E] {sp.DisplayName} Plakasını Raya Tak";
                            return;
                        }
                    }
                    else
                    {
                        var cart = hit.collider.GetComponentInParent<CartPhysics>();
                        if (cart != null)
                        {
                            hoveredCart = cart;
                            currentPrompt = "[Sol Tık / E] Ağırlığı Arabaya Yerleştir";
                            return;
                        }
                    }
                    continue;
                }

                // 2. Check VRPhysicalButton
                var btn = hit.collider.GetComponentInParent<VRPhysicalButton>();
                if (btn != null)
                {
                    hoveredButton = btn;
                    hoveredButton.AddHover();
                    currentPrompt = $"[Sol Tık / E] Butona Bas ({btn.gameObject.name})";
                    return;
                }

                // 3. Check Grabbable (weights, surface plates)
                var grab = hit.collider.GetComponentInParent<Grabbable>();
                if (grab != null && !hit.collider.isTrigger)
                {
                    hoveredGrabbable = grab;
                    var sp = grab.GetComponent<SurfacePlate>();
                    string label = sp != null ? sp.DisplayName : grab.gameObject.name;
                    currentPrompt = $"[Sol Tık / E] Al: {label}";
                    return;
                }

                // Stop raycast at solid geometry (table, walls, etc.)
                if (!hit.collider.isTrigger) break;
            }

            if (heldGrabbable != null && string.IsNullOrEmpty(currentPrompt))
            {
                currentPrompt = "[Sol Tık / E] Bırak   |   [Fare Tekerleği] Mesafe";
            }
        }

        private void HandleInteractionInput()
        {
            if (!cursorLocked) return;

            // Adjust hold distance with scroll wheel
            if (heldGrabbable != null)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.001f)
                {
                    holdDistance = Mathf.Clamp(holdDistance + scroll * 0.5f, minHoldDistance, maxHoldDistance);
                }
            }

            bool interactPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E);
            if (!interactPressed) return;

            // Case 1: Holding an object -> snap to target or release
            if (heldGrabbable != null)
            {
                if (hoveredSurfaceSlot != null)
                {
                    var plate = heldGrabbable.GetComponent<SurfacePlate>();
                    ReleaseHeldObject(zeroVelocity: true);
                    if (plate != null) hoveredSurfaceSlot.InstallPlate(plate);
                    return;
                }

                if (hoveredCart != null && heldRb != null)
                {
                    var rbToAttach = heldRb;
                    ReleaseHeldObject(zeroVelocity: true);
                    hoveredCart.AttachWeight(rbToAttach);
                    return;
                }

                ReleaseHeldObject(zeroVelocity: true);
                return;
            }

            // Case 2: Press VRPhysicalButton
            if (hoveredButton != null)
            {
                hoveredButton.RemotePress(ControllerHand.Right);
                return;
            }

            // Case 3: Pick up Grabbable
            if (hoveredGrabbable != null)
            {
                PickupGrabbable(hoveredGrabbable);
            }
        }

        private void PickupGrabbable(Grabbable grab)
        {
            if (grab == null) return;

            // If it's a SurfacePlate installed in a slot, uninstall it first
            var plate = grab.GetComponent<SurfacePlate>();
            if (plate != null && plate.IsInstalled)
            {
                var slot = Object.FindFirstObjectByType<SurfacePlateSlot>();
                if (slot != null && slot.InstalledPlate == plate)
                {
                    slot.UninstallPlate(true);
                }
            }

            // If it's a weight attached to CartPhysics, detach it first
            var rb = grab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                var cart = Object.FindFirstObjectByType<CartPhysics>();
                if (cart != null && cart.IsWeightAttached(rb))
                {
                    cart.DetachWeight(rb);
                }
            }

            heldGrabbable = grab;
            heldRb = rb;
            heldOriginalParent = grab.transform.parent;
            heldColliders = grab.GetComponentsInChildren<Collider>(true);

            if (heldRb != null)
            {
                heldWasKinematic = heldRb.isKinematic;
                heldUsedGravity = heldRb.useGravity;
                if (!heldRb.isKinematic)
                {
                    heldRb.linearVelocity = Vector3.zero;
                    heldRb.angularVelocity = Vector3.zero;
                }
                heldRb.isKinematic = true;
                heldRb.useGravity = false;
            }

            // Ignore collision with player CharacterController while held
            if (characterController != null && heldColliders != null)
            {
                foreach (var c in heldColliders)
                {
                    if (c != null) UnityEngine.Physics.IgnoreCollision(c, characterController, true);
                }
            }
        }

        private void UpdateHeldObject()
        {
            if (heldGrabbable == null || mainCam == null) return;

            Vector3 targetPos = mainCam.transform.position + mainCam.transform.forward * holdDistance + Vector3.down * 0.08f;
            heldGrabbable.transform.position = Vector3.Lerp(heldGrabbable.transform.position, targetPos, Time.deltaTime * holdSmoothSpeed);
        }

        private void ReleaseHeldObject(bool zeroVelocity = false)
        {
            if (heldGrabbable == null) return;

            if (characterController != null && heldColliders != null)
            {
                foreach (var c in heldColliders)
                {
                    if (c != null) UnityEngine.Physics.IgnoreCollision(c, characterController, false);
                }
            }

            if (heldRb != null)
            {
                heldRb.isKinematic = false;
                heldRb.useGravity = true;
                if (zeroVelocity)
                {
                    heldRb.linearVelocity = Vector3.zero;
                    heldRb.angularVelocity = Vector3.zero;
                }
            }

            heldGrabbable = null;
            heldRb = null;
            heldColliders = null;
        }

        private void OnDisable()
        {
            if (hoveredButton != null)
            {
                hoveredButton.RemoveHover();
                hoveredButton = null;
            }
            if (heldGrabbable != null) ReleaseHeldObject(true);
            if (cursorLocked) SetCursorLocked(false);
        }

        private void OnGUI()
        {
            if (!isDesktopActive) return;

            EnsureGuiStyles();

            // 1. Center crosshair dot (only when cursor is locked)
            if (cursorLocked)
            {
                bool hasTarget = hoveredButton != null || hoveredGrabbable != null || hoveredCart != null || hoveredSurfaceSlot != null;
                Color dotColor = hasTarget ? new Color(0.22f, 0.85f, 0.98f, 0.95f) : new Color(1f, 1f, 1f, 0.75f);
                crosshairStyle.normal.textColor = dotColor;
                GUI.Label(new Rect(Screen.width * 0.5f - 10f, Screen.height * 0.5f - 12f, 20f, 24f), hasTarget ? "◉" : "•", crosshairStyle);

                if (!string.IsNullOrEmpty(currentPrompt))
                {
                    GUI.Label(new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.5f + 18f, 440f, 30f), currentPrompt, promptStyle);
                }
            }
            else
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 210f, 18f, 420f, 30f),
                    "PC Test Modu — Kontrol için ekrana tıkla (veya Tab)", promptStyle);
            }

            // 2. Compact bottom-left control legend
            if (showHelpHud)
            {
                string help = "<b>PC TEST MODU</b> (VR Gözlük Takılınca Otomatik Kapanır)\n" +
                              "WASD: Yürü (Shift: Hızlı)  |  Fare: Bak  |  Sol Tık / E: Butona Bas / Tut-Bırak\n" +
                              "1 / 2 / 3: İstasyon 1-2-3'e Işınlan  |  R: Sıfırla  |  -/+: Açı  |  Esc: Fare  |  H: Gizle";
                GUI.Label(new Rect(14f, Screen.height - 76f, 640f, 64f), help, hudStyle);
            }
        }

        private void EnsureGuiStyles()
        {
            if (crosshairStyle == null)
            {
                crosshairStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.92f, 0.97f, 1f, 1f) }
                };
            }
            if (hudStyle == null)
            {
                hudStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 12,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 6, 6),
                    normal = { textColor = new Color(0.88f, 0.93f, 0.98f, 0.95f) }
                };
            }
        }
    }
}
