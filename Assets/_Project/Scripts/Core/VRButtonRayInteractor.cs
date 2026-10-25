using System.Collections.Generic;
using UnityEngine;
using BNG;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Lets the player press <see cref="VRPhysicalButton"/>s from a distance.
    /// Casts a ray from each controller (BNG UIPointer transform, or Grabber as fallback),
    /// draws a laser + cursor only while aiming at a button, highlights it, and presses it
    /// when the controller trigger is pulled.
    /// Auto-spawned by the first VRPhysicalButton, so no scene setup is required.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class VRButtonRayInteractor : MonoBehaviour
    {
        public static VRButtonRayInteractor Instance { get; private set; }

        private static readonly List<Grabber> grabberCache = new List<Grabber>();

        /// <summary>Cached list of player Grabbers (used by buttons for palm presses).</summary>
        public static IReadOnlyList<Grabber> Grabbers => grabberCache;

        [Header("Ray")]
        [SerializeField] private float maxDistance = 8f;
        [SerializeField] private LayerMask raycastMask = ~0;
        [Tooltip("Do not show the laser when the hand is closer than this to the button (use touch instead).")]
        [SerializeField] private float minDistance = 0.12f;

        [Header("Visuals")]
        [SerializeField] private Color laserColor = new Color(1f, 0.78f, 0.25f, 0.9f);
        [SerializeField] private float laserWidth = 0.004f;
        [SerializeField] private float cursorSize = 0.018f;

        private class HandRay
        {
            public ControllerHand hand;
            public Transform origin;
            public Grabber grabber;
            public LineRenderer line;
            public Transform cursor;
            public VRPhysicalButton hovered;
        }

        private HandRay left;
        private HandRay right;
        private readonly RaycastHit[] hits = new RaycastHit[24];
        private float nextSearchTime;
        private Material laserMaterial;

        public static void EnsureExists()
        {
            if (Instance != null || !Application.isPlaying) return;

            var existing = Object.FindFirstObjectByType<VRButtonRayInteractor>();
            if (existing != null)
            {
                Instance = existing;
                return;
            }

            new GameObject("[VRButtonRayInteractor]").AddComponent<VRButtonRayInteractor>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (laserMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
                laserMaterial = new Material(shader);
                laserMaterial.color = laserColor;
            }

            if (left == null || left.line == null) left = CreateHand(ControllerHand.Left);
            if (right == null || right.line == null) right = CreateHand(ControllerHand.Right);

            RefreshReferences();
        }

        private HandRay CreateHand(ControllerHand hand)
        {
            var h = new HandRay { hand = hand };

            var lineGo = new GameObject($"ButtonLaser_{hand}");
            lineGo.transform.SetParent(transform, false);
            h.line = lineGo.AddComponent<LineRenderer>();
            h.line.useWorldSpace = true;
            h.line.positionCount = 2;
            h.line.startWidth = laserWidth;
            h.line.endWidth = laserWidth * 0.5f;
            h.line.sharedMaterial = laserMaterial;
            h.line.startColor = laserColor;
            h.line.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0.35f);
            h.line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            h.line.receiveShadows = false;
            h.line.enabled = false;

            var cursorGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cursorGo.name = $"ButtonCursor_{hand}";
            Destroy(cursorGo.GetComponent<Collider>());
            cursorGo.transform.SetParent(transform, false);
            cursorGo.transform.localScale = Vector3.one * cursorSize;
            var rend = cursorGo.GetComponent<Renderer>();
            rend.sharedMaterial = laserMaterial;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            cursorGo.SetActive(false);
            h.cursor = cursorGo.transform;

            return h;
        }

        private void RefreshReferences()
        {
            grabberCache.Clear();
            grabberCache.AddRange(Object.FindObjectsByType<Grabber>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

            if (left != null) AssignHand(left);
            if (right != null) AssignHand(right);
        }

        private void AssignHand(HandRay h)
        {
            if (h.grabber == null)
            {
                foreach (var g in grabberCache)
                {
                    if (g != null && g.HandSide == h.hand) { h.grabber = g; break; }
                }
            }

            if (h.origin == null)
            {
                // Prefer BNG's UI pointer transform: it is aligned with the natural pointing direction.
                // Its transform follows the controller even if the GameObject is disabled.
                var pointers = Object.FindObjectsByType<UIPointer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var p in pointers)
                {
                    if (p != null && p.ControllerSide == h.hand) { h.origin = p.transform; break; }
                }

                if (h.origin == null && h.grabber != null)
                {
                    h.origin = h.grabber.transform;
                }
            }
        }

        private void Update()
        {
            EnsureInitialized();

            if (Time.unscaledTime >= nextSearchTime)
            {
                nextSearchTime = Time.unscaledTime + 2f;
                RefreshReferences();
            }

            var input = InputBridge.Instance;
            if (left != null) ProcessHand(left, input != null && input.LeftTriggerDown);
            if (right != null) ProcessHand(right, input != null && input.RightTriggerDown);
        }

        private void ProcessHand(HandRay h, bool triggerDown)
        {
            if (h == null) return;

            VRPhysicalButton target = null;
            Vector3 hitPoint = Vector3.zero;

            bool canPoint = h.origin != null && (h.grabber == null || !h.grabber.HoldingItem);
            if (canPoint)
            {
                target = Raycast(h.origin.position, h.origin.forward, out hitPoint, out float distance);
                if (target != null && distance < minDistance) target = null;
            }

            if (h.hovered != target)
            {
                if (h.hovered != null) h.hovered.RemoveHover();
                if (target != null) target.AddHover();
                h.hovered = target;
            }

            if (target != null)
            {
                if (h.line != null)
                {
                    h.line.enabled = true;
                    h.line.SetPosition(0, h.origin.position);
                    h.line.SetPosition(1, hitPoint);
                }
                if (h.cursor != null)
                {
                    h.cursor.gameObject.SetActive(true);
                    h.cursor.position = hitPoint;
                }

                if (triggerDown)
                {
                    target.RemotePress(h.hand);
                }
            }
            else
            {
                if (h.line != null) h.line.enabled = false;
                if (h.cursor != null) h.cursor.gameObject.SetActive(false);
            }
        }

        private VRPhysicalButton Raycast(Vector3 origin, Vector3 direction, out Vector3 hitPoint, out float distance)
        {
            hitPoint = Vector3.zero;
            distance = 0f;

            int count = UnityEngine.Physics.RaycastNonAlloc(origin, direction, hits, maxDistance, raycastMask, QueryTriggerInteraction.Collide);
            if (count == 0) return null;

            System.Array.Sort(hits, 0, count, RaycastHitComparer.Instance);

            for (int i = 0; i < count; i++)
            {
                Collider col = hits[i].collider;
                if (col == null) continue;

                // Never let the player's own hands / body block the ray
                if (col.GetComponentInParent<Grabber>() != null ||
                    col.GetComponentInParent<HandController>() != null ||
                    col.GetComponentInParent<BNGPlayerController>() != null ||
                    col is CharacterController)
                {
                    continue;
                }

                var button = col.GetComponentInParent<VRPhysicalButton>();
                if (button != null && button.isActiveAndEnabled)
                {
                    hitPoint = hits[i].point;
                    distance = hits[i].distance;
                    return button;
                }

                // Trigger volumes (sensors, weight slots...) should not block the laser
                if (col.isTrigger) continue;

                // A solid non-button object is in the way
                return null;
            }

            return null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (laserMaterial != null) Destroy(laserMaterial);
        }

        private class RaycastHitComparer : IComparer<RaycastHit>
        {
            public static readonly RaycastHitComparer Instance = new RaycastHitComparer();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }
    }
}
