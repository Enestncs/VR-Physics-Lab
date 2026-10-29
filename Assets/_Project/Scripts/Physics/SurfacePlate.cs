using UnityEngine;
using BNG;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    public enum SurfaceType
    {
        Ice,
        Wood,
        Rubber
    }

    /// <summary>
    /// Represents an interchangeable surface plate (Ice, Wood, Rubber)
    /// that can be picked up in VR and inserted into the friction track.
    /// </summary>
    [RequireComponent(typeof(Grabbable))]
    public class SurfacePlate : MonoBehaviour, IResetable
    {
        [Header("Surface Properties")]
        [SerializeField] private SurfaceType surfaceType = SurfaceType.Ice;
        [SerializeField] private string surfaceDisplayName = "Buz / Teflon";
        [Tooltip("Dynamic friction coefficient (\u03bc_k) representing this surface.")]
        [SerializeField] private float frictionCoefficient = 0.05f;
        [Tooltip("PhysicMaterial applied to the track when this plate is installed.")]
        [SerializeField] private PhysicsMaterial surfacePhysicsMaterial;

        [Header("Visual Feedback")]
        [SerializeField] private Material plateMaterial;
        [SerializeField] private Color plateColor = new Color(0.6f, 0.9f, 1f);
        [SerializeField] private string description = "Düşük sürtünme: Cisim uzun süre kayar.";

        private Grabbable grabbable;
        private Rigidbody rb;
        private Collider col;
        private Collider[] allColliders;
        private Transform handleTransform;
        private Vector3 initialHandleLocalPos = new Vector3(0f, 0.04f, 0f);
        private bool hasInitialHandlePos = false;
        private bool isInstalled = false;

        public SurfaceType Type => surfaceType;
        public string DisplayName => surfaceDisplayName;
        public float FrictionCoefficient => frictionCoefficient;
        public PhysicsMaterial PhysicsMaterial => surfacePhysicsMaterial;
        public Material PlateMaterial => plateMaterial;
        public Color PlateColor => plateColor;
        public string Description => description;
        public bool IsInstalled => isInstalled;
        public Grabbable Grabbable => grabbable != null ? grabbable : (grabbable = GetComponent<Grabbable>());
        public Rigidbody Rigidbody => rb != null ? rb : (rb = GetComponent<Rigidbody>());
        public Collider Collider => col != null ? col : (col = GetComponentInChildren<Collider>());
        public Collider[] AllColliders => allColliders != null && allColliders.Length > 0 ? allColliders : (allColliders = GetComponentsInChildren<Collider>(true));

        private void Awake()
        {
            grabbable = GetComponent<Grabbable>();
            rb = GetComponent<Rigidbody>();

            EnsureGrabSetup();

            handleTransform = transform.Find("Handle");
            if (handleTransform != null)
            {
                initialHandleLocalPos = handleTransform.localPosition;
                hasInitialHandlePos = true;
            }
        }

        /// <summary>
        /// Ensures BNG Grabbable and colliders are properly configured on the root object
        /// so VR hand grabbers and remote grabbers always detect this surface plate.
        /// </summary>
        private void EnsureGrabSetup()
        {
            col = GetComponent<Collider>();
            if (col == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 0.022f, 0f);
                box.size = new Vector3(0.20f, 0.07f, 0.16f);
                col = box;
            }

            // Ensure any child colliders (if present) forward grab detection to this Grabbable
            var childCols = GetComponentsInChildren<Collider>(true);
            foreach (var c in childCols)
            {
                if (c != null && c.gameObject != gameObject)
                {
                    var gc = c.GetComponent<GrabbableChild>();
                    if (gc == null)
                    {
                        gc = c.gameObject.AddComponent<GrabbableChild>();
                    }
                    gc.ParentGrabbable = grabbable;
                }
            }

            if (grabbable != null)
            {
                grabbable.ForceDisableKinematicOnDrop = true;
                grabbable.MakeChildCollidersGrabbable = true;
            }

            allColliders = GetComponentsInChildren<Collider>(true);
        }

        public void SetInstalled(bool installed)
        {
            isInstalled = installed;

            if (handleTransform == null)
            {
                handleTransform = transform.Find("Handle");
            }

            if (handleTransform != null)
            {
                if (!hasInitialHandlePos)
                {
                    initialHandleLocalPos = new Vector3(0f, 0.04f, 0f);
                    hasInitialHandlePos = true;
                }

                // When installed on the track, shift the handle to the back rail edge
                // so the sliding friction cart never clips through the handle visually
                handleTransform.localPosition = installed
                    ? new Vector3(0f, 0.035f, 0.11f)
                    : initialHandleLocalPos;
            }
        }

        public void ResetToInitialState()
        {
            SetInstalled(false);
        }
    }
}
