using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRPhysicsLab.Measurement;
using VRPhysicsLab.Physics;

namespace VRPhysicsLab.UI
{
    /// <summary>
    /// Formats and updates the digital lab monitor dashboard in real-time.
    /// Provides telemetry metrics, experimental parameters, theoretical calculations, and visual error indicators.
    /// </summary>
    public class DigitalDisplayUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private MeasurementSystem measurementSystem;
        [SerializeField] private CartPhysics cart;
        [SerializeField] private ForceApplicator forceApplicator;

        [Header("Status Header")]
        [SerializeField] private TMP_Text statusBadgeText;
        [SerializeField] private Image statusBadgeBg;

        [Header("Telemetry Cards (Live Measurements)")]
        [SerializeField] private TMP_Text timeValueText;
        [SerializeField] private TMP_Text speedValueText;
        [SerializeField] private TMP_Text accelValueText;
        [SerializeField] private TMP_Text distanceValueText;
        [SerializeField] private Image distanceFillBar;

        [Header("Experiment Parameters (Setup & Theory)")]
        [SerializeField] private TMP_Text forceValueText;
        [SerializeField] private TMP_Text massValueText;
        [SerializeField] private TMP_Text theoreticalAccelText;
        [SerializeField] private TMP_Text errorValueText;

        [Header("Bottom Banner")]
        [SerializeField] private TMP_Text hintText;

        [Header("Fallback Single Text (Optional)")]
        [SerializeField] private TMP_Text fallbackDashboardText;

        [Header("Track Configuration")]
        [SerializeField] private float totalTrackDistance = 1.40f;

        private void Start()
        {
            if (measurementSystem == null)
            {
                measurementSystem = Object.FindFirstObjectByType<MeasurementSystem>();
            }

            if (cart == null)
            {
                cart = Object.FindFirstObjectByType<CartPhysics>();
            }

            if (forceApplicator == null)
            {
                forceApplicator = Object.FindFirstObjectByType<ForceApplicator>();
            }

            if (measurementSystem != null)
            {
                measurementSystem.onMeasurementUpdated.AddListener(UpdateDisplay);
                measurementSystem.onMeasurementCompleted.AddListener(OnCompleted);
            }

            if (cart != null)
            {
                cart.onMassChanged.AddListener(OnMassChanged);
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

            // Values
            float timeVal = measurementSystem != null ? measurementSystem.ElapsedTime : 0f;
            float speedVal = measurementSystem != null ? measurementSystem.CurrentVelocity : 0f;
            float accelVal = 0f;
            if (measurementSystem != null)
            {
                accelVal = state == MeasurementState.Completed
                    ? measurementSystem.LastResult.averageAcceleration
                    : measurementSystem.AverageAcceleration;
            }

            float distVal = measurementSystem != null ? measurementSystem.DistanceTraveled : 0f;
            float massVal = cart != null ? cart.TotalMass : 1.0f;
            float forceVal = forceApplicator != null ? forceApplicator.NetForce : 4.0f;
            float theoreticalAccel = massVal > 0.001f ? (forceVal / massVal) : 0f;

            float relativeError = 0f;
            if (theoreticalAccel > 0.001f && state == MeasurementState.Completed)
            {
                relativeError = Mathf.Abs(accelVal - theoreticalAccel) / theoreticalAccel * 100f;
            }

            // Status Badge Formatting
            string statusStr;
            Color badgeBgColor;
            string hintStr;

            switch (state)
            {
                case MeasurementState.Idle:
                    statusStr = "● HAZIR";
                    badgeBgColor = new Color(0.06f, 0.45f, 0.25f, 0.85f); // Rich emerald green
                    hintStr = "Fırlatmak için masadaki <color=#FF5555><b>KIRMIZI</b></color> butona basın.";
                    break;
                case MeasurementState.Measuring:
                    statusStr = "▶ HAREKET HALİNDE...";
                    badgeBgColor = new Color(0.05f, 0.35f, 0.65f, 0.85f); // Vibrant cyan blue
                    hintStr = "Araba bitiş kapısına doğru ilerliyor...";
                    break;
                case MeasurementState.Completed:
                    statusStr = "✔ TAMAMLANDI";
                    badgeBgColor = new Color(0.65f, 0.45f, 0.05f, 0.85f); // Golden amber
                    hintStr = "Ölçüm bitti! Yeniden başlatmak için <color=#FFCC00><b>SARI</b></color> butona basın.";
                    break;
                default:
                    statusStr = "BEKLENİYOR";
                    badgeBgColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);
                    hintStr = "";
                    break;
            }

            // Update Discrete UI Components
            if (statusBadgeText != null) statusBadgeText.text = statusStr;
            if (statusBadgeBg != null) statusBadgeBg.color = badgeBgColor;

            if (timeValueText != null) timeValueText.text = $"{timeVal:F2} s";
            if (speedValueText != null) speedValueText.text = $"{speedVal:F2} m/s";
            if (accelValueText != null) accelValueText.text = $"{accelVal:F2} m/s²";

            if (distanceValueText != null)
                distanceValueText.text = $"{distVal:F2} / {totalTrackDistance:F2} m";

            if (distanceFillBar != null)
            {
                float progress = Mathf.Clamp01(distVal / totalTrackDistance);
                distanceFillBar.fillAmount = progress;
            }

            if (forceValueText != null) forceValueText.text = $"{forceVal:F1} N";
            if (massValueText != null) massValueText.text = $"{massVal:F1} kg";
            if (theoreticalAccelText != null) theoreticalAccelText.text = $"{theoreticalAccel:F2} m/s²";

            if (errorValueText != null)
            {
                if (state == MeasurementState.Completed)
                {
                    errorValueText.text = $"% {relativeError:F1}";
                    errorValueText.color = relativeError < 5f ? new Color(0.2f, 0.9f, 0.4f) : new Color(1f, 0.8f, 0.2f);
                }
                else
                {
                    errorValueText.text = "-- %";
                    errorValueText.color = new Color(0.6f, 0.7f, 0.8f);
                }
            }

            if (hintText != null) hintText.text = hintStr;

            // Fallback dashboard if used
            if (fallbackDashboardText != null)
            {
                fallbackDashboardText.text =
                    $"<size=18><color=#94A3B8>VR FİZİK LAB</color> | <color=#FFFFFF>NEWTON 2. YASA</color></size>   <align=right><color=#00FFAA><b>{statusStr}</b></color></align>\n\n" +
                    $"<b><color=#38BDF8>CANLI ÖLÇÜMLER</color></b>                   <b><color=#F59E0B>DENEY PARAMETRELERİ</color></b>\n" +
                    $"Süre: <b>{timeVal:F2} s</b>                     Kuvvet: <b>{forceVal:F1} N</b>\n" +
                    $"Hız : <b>{speedVal:F2} m/s</b>                  Kütle : <b>{massVal:F1} kg</b>\n" +
                    $"İvme: <b>{accelVal:F2} m/s²</b>                 Teorik a: <b>{theoreticalAccel:F2} m/s²</b>\n" +
                    $"Yol : <b>{distVal:F2} / {totalTrackDistance:F2} m</b>              Hata: <b>% {relativeError:F1}</b>\n\n" +
                    $"<size=13><color=#94A3B8>{hintStr}</color></size>";
            }
        }

        private void OnCompleted(MeasurementData result)
        {
            UpdateDisplay();
        }

        private void OnMassChanged(float newMass)
        {
            UpdateDisplay();
        }

        public void BindComponents(
            TMP_Text statusText, Image statusBg,
            TMP_Text timeText, TMP_Text speedText, TMP_Text accelText,
            TMP_Text distText, Image distFill,
            TMP_Text forceText, TMP_Text massText, TMP_Text theoText, TMP_Text errText,
            TMP_Text hint)
        {
            statusBadgeText = statusText;
            statusBadgeBg = statusBg;
            timeValueText = timeText;
            speedValueText = speedText;
            accelValueText = accelText;
            distanceValueText = distText;
            distanceFillBar = distFill;
            forceValueText = forceText;
            massValueText = massText;
            theoreticalAccelText = theoText;
            errorValueText = errText;
            hintText = hint;
            UpdateDisplay();
        }
    }
}
