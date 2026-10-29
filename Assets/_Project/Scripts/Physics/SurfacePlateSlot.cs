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

        [Header("Events")]
        public UnityEvent<SurfacePlate> onSurfaceInstalled;
        public UnityEvent onSurfaceRemoved;

        private SurfacePlate installedPlate = null;
        private readonly List<SurfacePlate> candidatePlates = new List<SurfacePlate>();

        private Transform originalPlateParent;
        private bool wasKinematic;

        public SurfacePlate InstalledPlate => installedPlate;
        public bool HasSurfaceInstalled => installedPlate != null;

        private void Awake()
        {
            if (snapTransform == null)
            {
                snapTransform = transform;
            }
        }

        private void Start()
        {
            ApplyPhysicsMaterial(defaultPhysicsMaterial);
            ApplyVisualMaterial(defaultVisualMaterial);
        }

        private void OnTriggerEnter(Collider other)
        {
            var plate = other.GetComponentInParent<SurfacePlate>();
            if (plate == null) return;

            // Reject hands/controllers
            if (other.GetComponentInParent<Grabber>() != null ||
                other.GetComponentInParent<BNGPlayerController>() != null)
            {
                return;
            }

            if (!candidatePlates.Contains(plate))
            {
                candidatePlates.Add(plate);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var plate = other.GetComponentInParent<SurfacePlate>();
            if (plate == null) return;

            // If this plate is currently locked into the slot and NOT being held by the user, don't remove it
            if (plate == installedPlate && (plate.Grabbable == null || !plate.Grabbable.BeingHeld))
            {
                return;
            }

            if (candidatePlates.Contains(plate))
            {
                candidatePlates.Remove(plate);
            }

            if (plate == installedPlate)
            {
                UninstallPlate();
            }
        }

        private void FixedUpdate()
        {
            // Check if installed plate was grabbed off the track
            if (installedPlate != null)
            {
                if (installedPlate.Grabbable != null && installedPlate.Grabbable.BeingHeld)
                {
                    UninstallPlate();
                }
            }

            // Check candidates for a plate dropped into the slot
            if (installedPlate == null && candidatePlates.Count > 0)
            {
                for (int i = candidatePlates.Count - 1; i >= 0; i--)
                {
                    var plate = candidatePlates[i];
                    if (plate == null)
                    {
                        candidatePlates.RemoveAt(i);
                        continue;
                    }

                    bool isHeld = plate.Grabbable != null && plate.Grabbable.BeingHeld;
                    if (!isHeld)
                    {
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
            if (plate == null || installedPlate != null) return;

            installedPlate = plate;
            originalPlateParent = plate.transform.parent;

            var rb = plate.Rigidbody;
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // Align to snap slot
            plate.transform.SetParent(snapTransform != null ? snapTransform : transform, true);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localRotation = Quaternion.identity;

            plate.SetInstalled(true);

            // Apply new friction physics and visual material to track
            ApplyPhysicsMaterial(plate.PhysicsMaterial);
            ApplyVisualMaterial(plate.PlateMaterial);

            UnityEngine.Physics.SyncTransforms();

            onSurfaceInstalled?.Invoke(plate);
            Debug.Log($"[SurfacePlateSlot] Installed surface plate: {plate.DisplayName} (\u03bc={plate.FrictionCoefficient})");
        }

        /// <summary>
        /// Releases the currently installed surface plate.
        /// </summary>
        public void UninstallPlate()
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
                rb.isKinematic = wasKinematic;
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
            UninstallPlate();
            candidatePlates.Clear();
            ApplyPhysicsMaterial(defaultPhysicsMaterial);
            ApplyVisualMaterial(defaultVisualMaterial);
        }
    }
}
