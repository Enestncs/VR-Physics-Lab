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
        public Collider Collider => col != null ? col : (col = GetComponent<Collider>());

        private void Awake()
        {
            grabbable = GetComponent<Grabbable>();
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }

        public void SetInstalled(bool installed)
        {
            isInstalled = installed;
        }

        public void ResetToInitialState()
        {
            isInstalled = false;
        }
    }
}
