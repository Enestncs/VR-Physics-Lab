using UnityEngine;
using UnityEngine.Events;
using TMPro;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Controls an adjustable inclined plane ramp whose tilt angle can be changed
    /// in discrete steps (10° - 45°), dynamically animating the ramp pivot, support jack,
    /// and angle readout display.
    /// </summary>
    public class AdjustableRamp : MonoBehaviour, IResetable
    {
        [Header("Angle Configuration")]
        [SerializeField] private float defaultAngle = 20f;
        [SerializeField] private float minAngle = 10f;
        [SerializeField] private float maxAngle = 45f;
        [SerializeField] private float angleStep = 5f;
        [SerializeField] private float currentAngle = 20f;
        [SerializeField] private float rotateSpeed = 90f;

        [Header("Visual Components")]
        [Tooltip("Hinge pivot at the lower-right end of the ramp. Rotates around Z by -angle.")]
        [SerializeField] private Transform rampPivot;

        [Tooltip("Optional telescoping support column under the elevated end of the ramp.")]
        [SerializeField] private Transform supportColumn;

        [Tooltip("Local X offset of the support column attachment point along the ramp (negative = towards top-left).")]
        [SerializeField] private float supportAttachLocalX = -1.25f;

        [Tooltip("Local Y height of the table surface beneath the support column (relative to this ramp root).")]
        [SerializeField] private float tableSurfaceLocalY = 0.905f;

        [Tooltip("Optional world-space angle label near the hinge.")]
        [SerializeField] private TMP_Text angleReadoutText;

        [Header("Events")]
        public UnityEvent<float> onAngleChanged;

        private float displayedAngle;

        public float CurrentAngle => currentAngle;
        public float MinAngle => minAngle;
        public float MaxAngle => maxAngle;
        public float AngleRadians => currentAngle * Mathf.Deg2Rad;
        public Transform RampPivot => rampPivot;

        private void Awake()
        {
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
            displayedAngle = currentAngle;
            ApplyRampGeometry(displayedAngle);
            UpdateAngleText();
        }

        private void Update()
        {
            if (Mathf.Abs(displayedAngle - currentAngle) > 0.01f)
            {
                displayedAngle = Mathf.MoveTowards(displayedAngle, currentAngle, rotateSpeed * Time.deltaTime);
                ApplyRampGeometry(displayedAngle);
            }
        }

        [ContextMenu("Increase Angle (+5°)")]
        public void IncreaseAngle()
        {
            SetAngle(currentAngle + angleStep);
        }

        [ContextMenu("Decrease Angle (-5°)")]
        public void DecreaseAngle()
        {
            SetAngle(currentAngle - angleStep);
        }

        public void SetAngle(float newAngle)
        {
            float clamped = Mathf.Clamp(Mathf.Round(newAngle / angleStep) * angleStep, minAngle, maxAngle);
            if (Mathf.Approximately(clamped, currentAngle)) return;

            currentAngle = clamped;
            UpdateAngleText();
            onAngleChanged?.Invoke(currentAngle);
            Debug.Log($"[AdjustableRamp] Ramp angle set to {currentAngle:F0}°");
        }

        public void ApplyImmediate()
        {
            displayedAngle = currentAngle;
            ApplyRampGeometry(displayedAngle);
            UpdateAngleText();
        }

        private void ApplyRampGeometry(float angleDeg)
        {
            if (rampPivot != null)
            {
                // Negative Z rotation lifts the -X (top-left) end of the ramp upward
                rampPivot.localRotation = Quaternion.Euler(0f, 0f, -angleDeg);
            }

            if (supportColumn != null && rampPivot != null)
            {
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector3 pivotLocal = rampPivot.localPosition;
                // Position of attachment point on the underside of the tilted ramp
                float attachX = pivotLocal.x + supportAttachLocalX * Mathf.Cos(rad);
                float attachY = pivotLocal.y - supportAttachLocalX * Mathf.Sin(rad) - 0.015f;

                float colHeight = Mathf.Max(0.02f, attachY - tableSurfaceLocalY);
                supportColumn.localPosition = new Vector3(attachX, tableSurfaceLocalY + colHeight * 0.5f, pivotLocal.z);
                Vector3 s = supportColumn.localScale;
                // Unity's PrimitiveType.Cylinder is 2 units tall at scale.y = 1, so divide by 2
                supportColumn.localScale = new Vector3(s.x, colHeight * 0.5f, s.z);
            }
        }

        private void UpdateAngleText()
        {
            if (angleReadoutText != null)
            {
                angleReadoutText.text = $"<b>θ = {currentAngle:F0}°</b>";
            }
        }

        public void ResetToInitialState()
        {
            // Keep the user's currently chosen angle on normal experiment reset?
            // No, GDD 6.2 says reset restores consistent state; keep angle clamped and geometry synced.
            ApplyImmediate();
        }
    }
}
