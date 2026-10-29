using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BNG;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Snaps interchangeable surface plates onto the friction track, updates
    /// the track's physical material, and notifies listeners of surface changes.
    /// </summary>
    public class SurfacePlateSlot : MonoBehaviour, IResetable
    {
        [Header("Target & Configuration")]
        [Tooltip("The collider of the track that the cart rolls on.")]
        [SerializeField] private Collider trackSurfaceCollider;

        [Tooltip("Optional visual renderer of the track surface to change appearance to match the installed plate.")]
        [SerializeField] private Renderer trackSurfaceRenderer;

        [Tooltip("Default visual material when no plate is installed.")]
        [SerializeField] private Material defaultVisualMaterial;

        [Tooltip("Default physics material when no surface plate is installed.")]
        [SerializeField] private PhysicsMaterial defaultPhysicsMaterial;

        [Tooltip("Snap position and orientation for the installed plate.")]
        [SerializeField] private Transform snapTransform;

        [Tooltip("Local offset applied when snapping the plate so its surface sits flush with the track.")]
        [SerializeField] private Vector3 snapLocalOffset = new Vector3(0f, -0.016f, 0f);

        [Header("Events")]
        public UnityEvent<SurfacePlate> onSurfaceInstalled;
        public UnityEvent onSurfaceRemoved;

        private SurfacePlate installedPlate = null;
        private readonly List<SurfacePlate> candidatePlates = new List<SurfacePlate>();

        private Transform originalPlateParent;
        private RigidbodyInterpolation originalInterpolation;

        private Collider[] trackAndCartColliders;
        private FrictionCart frictionCart;
        private FrictionImpulseLauncher impulseLauncher;
        private VRPhysicsLab.Measurement.FrictionMeasurementSystem measurementSystem;

        public SurfacePlate InstalledPlate => installedPlate;
        public bool HasSurfaceInstalled => installedPlate != null;

        private void Awake()
        {
            if (snapTransform == null)
            {
                snapTransform = transform;
            }
            CacheTrackAndCartColliders();
        }

        private void Start()
        {
            if (frictionCart == null) frictionCart = Object.FindFirstObjectByType<FrictionCart>();
            if (impulseLauncher == null) impulseLauncher = Object.FindFirstObjectByType<FrictionImpulseLauncher>();
            if (measurementSystem == null) measurementSystem = Object.FindFirstObjectByType<VRPhysicsLab.Measurement.FrictionMeasurementSystem>();

            CacheTrackAndCartColliders();

            if (installedPlate == null)
            {
                ApplyPhysicsMaterial(defaultPhysicsMaterial);
                ApplyVisualMaterial(defaultVisualMaterial);
            }
        }

        private void CacheTrackAndCartColliders()
        {
            var colList = new List<Collider>();

            var trackRoot = transform.parent != null ? transform.parent : transform;
            foreach (var c in trackRoot.GetComponentsInChildren<Collider>(true))
            {
                if (c != null && !c.isTrigger && c.GetComponentInParent<SurfacePlate>() == null)
                {
                    colList.Add(c);
                }
            }

            if (frictionCart == null) frictionCart = Object.FindFirstObjectByType<FrictionCart>();
            if (frictionCart != null)
            {
                foreach (var c in frictionCart.GetComponentsInChildren<Collider>(true))
                {
                    if (c != null && !c.isTrigger)
                    {
                        colList.Add(c);
                    }
                }
            }

            trackAndCartColliders = colList.ToArray();
        }

        private void SetPlateCollisionIgnored(SurfacePlate plate, bool ignore)
        {
            if (plate == null) return;
            if (trackAndCartColliders == null || trackAndCartColliders.Length == 0)
            {
                CacheTrackAndCartColliders();
            }

            var plateCols = plate.AllColliders;
            if (plateCols == null || trackAndCartColliders == null) return;

            foreach (var pc in plateCols)
            {
                if (pc == null) continue;
                foreach (var tc in trackAndCartColliders)
                {
                    if (tc == null || pc == tc) continue;
                    UnityEngine.Physics.IgnoreCollision(pc, tc, ignore);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var plate = other.GetComponentInParent<SurfacePlate>();
            if (plate == null) return;

            // Reject hands/controllers
            if (other.GetComponentInParent<Grabber>() != null ||
                other.GetComponentInParent<HandController>() != null ||
                other.GetComponentInParent<BNGPlayerController>() != null)
            {
                return;
            }

            // Ignore collision between hovering candidate plate and track/cart so VR hand never fights the rails
            SetPlateCollisionIgnored(plate, true);

            if (!candidatePlates.Contains(plate))
            {
                candidatePlates.Add(plate);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var plate = other.GetComponentInParent<SurfacePlate>();
            if (plate == null) return;

            bool isHeld = plate.Grabbable != null && plate.Grabbable.BeingHeld;

            // If this plate is currently locked into the slot and NOT being held by the user, don't remove it
            if (plate == installedPlate && !isHeld)
            {
                return;
            }

            if (candidatePlates.Contains(plate))
            {
                candidatePlates.Remove(plate);
            }

            if (plate == installedPlate)
            {
                UninstallPlate(isHeld);
            }
            else
            {
                SetPlateCollisionIgnored(plate, false);
            }
        }

        private void Update()
        {
            // Immediately release the installed plate in Update as soon as the player grabs it in VR
            if (installedPlate != null && installedPlate.Grabbable != null && installedPlate.Grabbable.BeingHeld)
            {
                UninstallPlate(true);
            }
        }

        private void FixedUpdate()
        {
            // Check if installed plate was grabbed off the track
            if (installedPlate != null)
            {
                if (installedPlate.Grabbable != null && installedPlate.Grabbable.BeingHeld)
                {
                    UninstallPlate(true);
                }
            }

            // Check candidates for a newly released plate dropped into the slot
            if (candidatePlates.Count > 0)
            {
                for (int i = candidatePlates.Count - 1; i >= 0; i--)
                {
                    var plate = candidatePlates[i];
                    if (plate == null)
                    {
                        candidatePlates.RemoveAt(i);
                        continue;
                    }

                    if (plate == installedPlate)
                    {
                        continue;
                    }

                    bool isHeld = plate.Grabbable != null && plate.Grabbable.BeingHeld;
                    if (!isHeld)
                    {
                        // If another plate was already installed, swap it out and return the old plate to the tray
                        if (installedPlate != null && installedPlate != plate)
                        {
                            var oldPlate = installedPlate;
                            UninstallPlate(false);
                            if (candidatePlates.Contains(oldPlate))
                            {
                                candidatePlates.Remove(oldPlate);
                            }
                            var resetable = oldPlate.GetComponent<ResetableObject>();
                            if (resetable != null)
                            {
                                resetable.ResetToInitialState();
                            }
                        }

                        InstallPlate(plate);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Snaps and locks a surface plate onto the track.
        /// </summary>
        public void InstallPlate(SurfacePlate plate)
        {
            if (plate == null || installedPlate == plate) return;

            if (installedPlate != null)
            {
                UninstallPlate(false);
            }

            installedPlate = plate;
            originalPlateParent = plate.transform.parent;

            var rb = plate.Rigidbody;
            if (rb != null)
            {
                originalInterpolation = rb.interpolation;
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.None;
            }

            // Ignore collisions between installed plate and cart/track so cart glides unobstructed
            SetPlateCollisionIgnored(plate, true);

            // Align flush to snap slot
            Transform targetSnap = snapTransform != null ? snapTransform : transform;
            plate.transform.SetParent(targetSnap, true);
            plate.transform.localPosition = snapLocalOffset;
            plate.transform.localRotation = Quaternion.identity;

            plate.SetInstalled(true);

            // Apply new friction physics and visual material to track
            ApplyPhysicsMaterial(plate.PhysicsMaterial);
            ApplyVisualMaterial(plate.PlateMaterial);

            UnityEngine.Physics.SyncTransforms();

            // If previous run was already completed, auto-reset cart and launcher for the new surface
            if (measurementSystem != null && measurementSystem.State == VRPhysicsLab.Measurement.MeasurementState.Completed)
            {
                frictionCart?.ResetToInitialState();
                impulseLauncher?.ResetToInitialState();
                measurementSystem.ResetToInitialState();
            }

            onSurfaceInstalled?.Invoke(plate);
            Debug.Log($"[SurfacePlateSlot] Installed surface plate: {plate.DisplayName} (\u03bc={plate.FrictionCoefficient})");
        }

        /// <summary>
        /// Releases the currently installed surface plate.
        /// </summary>
        public void UninstallPlate(bool beingGrabbedByPlayer = false)
        {
            if (installedPlate == null) return;

            var plate = installedPlate;
            installedPlate = null;

            plate.SetInstalled(false);

            if (plate.transform.parent == snapTransform || plate.transform.parent == transform)
            {
                plate.transform.SetParent(originalPlateParent, true);
            }

            var rb = plate.Rigidbody;
            if (rb != null)
            {
                rb.isKinematic = false;
                if (beingGrabbedByPlayer || (plate.Grabbable != null && plate.Grabbable.BeingHeld))
                {
                    rb.useGravity = false;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }
                else
                {
                    rb.useGravity = true;
                    rb.interpolation = originalInterpolation;
                }
            }

            if (!beingGrabbedByPlayer && !candidatePlates.Contains(plate))
            {
                SetPlateCollisionIgnored(plate, false);
            }

            // Revert track to default materials
            ApplyPhysicsMaterial(defaultPhysicsMaterial);
            ApplyVisualMaterial(defaultVisualMaterial);

            onSurfaceRemoved?.Invoke();
            Debug.Log("[SurfacePlateSlot] Uninstalled surface plate.");
        }

        private void ApplyPhysicsMaterial(PhysicsMaterial mat)
        {
            if (trackSurfaceCollider != null)
            {
                trackSurfaceCollider.material = mat;
            }
        }

        private void ApplyVisualMaterial(Material mat)
        {
            if (trackSurfaceRenderer != null && mat != null)
            {
                trackSurfaceRenderer.material = mat;
            }
        }

        public void ResetToInitialState()
        {
            UninstallPlate(false);
            foreach (var p in candidatePlates)
            {
                if (p != null) SetPlateCollisionIgnored(p, false);
            }
            candidatePlates.Clear();
            ApplyPhysicsMaterial(defaultPhysicsMaterial);
            ApplyVisualMaterial(defaultVisualMaterial);
        }
    }
}
