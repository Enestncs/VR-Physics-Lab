using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRPhysicsLab.Measurement;
using VRPhysicsLab.Physics;

namespace VRPhysicsLab.UI
{
    /// <summary>
    /// Formats and updates the digital telemetry dashboard for Station 3 (Inclined Plane Experiment).
    /// Displays live descent time, incline distance, velocity, acceleration, ramp angle, and theoretical g*sin(theta).
    /// </summary>
    public class InclinedDisplayUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private InclinedMeasurementSystem measurementSystem;
        [SerializeField] private AdjustableRamp ramp;
        [SerializeField] private InclinedCart cart;

        [Header("Status Header")]
        [SerializeField] private TMP_Text statusBadgeText;
        [SerializeField] private Image statusBadgeBg;

        [Header("Telemetry Cards")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private TMP_Text accelText;

        [Header("Theory Cards")]
        [SerializeField] private TMP_Text angleText;
        [SerializeField] private TMP_Text parallelForceText;
        [SerializeField] private TMP_Text theoAccelText;
        [SerializeField] private TMP_Text errorText;

        [Header("Bottom Banner")]
        [SerializeField] private TMP_Text hintText;

        private void Start()
        {
            if (measurementSystem == null) measurementSystem = Object.FindFirstObjectByType<InclinedMeasurementSystem>();
            if (ramp == null) ramp = Object.FindFirstObjectByType<AdjustableRamp>();
            if (cart == null) cart = Object.FindFirstObjectByType<InclinedCart>();

            if (measurementSystem != null)
            {
                measurementSystem.onMeasurementUpdated.AddListener(UpdateDisplay);
                measurementSystem.onMeasurementCompleted.AddListener(OnCompleted);
            }
            if (ramp != null)
            {
                ramp.onAngleChanged.AddListener(OnAngleChanged);
            }

            UpdateDisplay();
        }

        private void Update()
        {
            if (measurementSystem != null && measurementSystem.State == MeasurementState.Measuring)
            {
                UpdateDisplay();
            }
        }

        public void UpdateDisplay()
        {
            MeasurementState state = measurementSystem != null ? measurementSystem.State : MeasurementState.Idle;

            float angleDeg = ramp != null ? ramp.CurrentAngle : 20f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float g = cart != null ? cart.Gravity : 9.81f;
            float m = cart != null ? cart.Mass : 1.0f;

            float timeVal = measurementSystem != null ? measurementSystem.ElapsedTime : 0f;
            float distVal = measurementSystem != null ? measurementSystem.CurrentDistance : 0f;
            float speedVal = measurementSystem != null ? measurementSystem.CurrentSpeed : 0f;
            float theoAccel = g * Mathf.Sin(angleRad);
            float parallelForce = m * theoAccel;
            float expAccel = 0f;
            float relError = 0f;

            if (state == MeasurementState.Completed && measurementSystem != null)
            {
                var res = measurementSystem.LastResult;
                timeVal = res.elapsedTime;
                distVal = res.distanceTraveled;
                speedVal = res.finalVelocity;
                expAccel = res.experimentalAcceleration;
                theoAccel = res.theoreticalAcceleration;
                parallelForce = res.parallelForce;
                relError = res.relativeError;
            }
            else if (state == MeasurementState.Measuring && timeVal > 0.02f)
            {
                expAccel = speedVal / timeVal;
            }

            string statusStr;
            Color badgeBgColor;
            string hintStr;

            switch (state)
            {
                case MeasurementState.Idle:
                    statusStr = "● HAZIR";
                    badgeBgColor = new Color(0.06f, 0.45f, 0.25f, 0.85f);
                    hintStr = "Açıyı <color=#38BDF8><b>MAVİ (+/-)</b></color> butonlarla ayarlayın, bırakmak için <color=#FF5555><b>KIRMIZI</b></color> butona basın.";
                    break;
                case MeasurementState.Measuring:
                    statusStr = ">> İNİŞ HALİNDE...";
                    badgeBgColor = new Color(0.05f, 0.35f, 0.65f, 0.85f);
                    hintStr = $"θ = {angleDeg:F0}° eğimde yer çekimi bileşeni (mg·sinθ) arabayı hızlandırıyor...";
                    break;
                case MeasurementState.Completed:
                    statusStr = "● TAMAMLANDI";
                    badgeBgColor = new Color(0.65f, 0.45f, 0.05f, 0.85f);
                    hintStr = "Ölçüm bitti! Farklı açı denemek için <color=#38BDF8><b>AÇI (+/-)</b></color> veya <color=#FFCC00><b>SARI</b></color> sıfırlama butonuna basın.";
                    break;
                default:
                    statusStr = "BEKLENİYOR";
                    badgeBgColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);
                    hintStr = "";
                    break;
            }

            if (statusBadgeText != null) statusBadgeText.text = statusStr;
            if (statusBadgeBg != null) statusBadgeBg.color = badgeBgColor;

            if (timeText != null) timeText.text = $"{timeVal:F2} s";
            if (distanceText != null) distanceText.text = $"{distVal:F2} m";
            if (speedText != null) speedText.text = $"{speedVal:F2} m/s";
            if (accelText != null) accelText.text = state == MeasurementState.Idle ? "--" : $"{expAccel:F2} m/s²";

            if (angleText != null) angleText.text = $"<b>{angleDeg:F0}°</b>  (sinθ={Mathf.Sin(angleRad):F2})";
            if (parallelForceText != null) parallelForceText.text = $"{parallelForce:F2} N";
            if (theoAccelText != null) theoAccelText.text = $"{theoAccel:F2} m/s²";
            if (errorText != null) errorText.text = state == MeasurementState.Completed ? $"%{relError:F1}" : "-- %";

            if (hintText != null) hintText.text = hintStr;
        }

        private void OnCompleted(InclinedMeasurementResult res) => UpdateDisplay();
        private void OnAngleChanged(float angle) => UpdateDisplay();
    }
}
