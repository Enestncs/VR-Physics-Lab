using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRPhysicsLab.Measurement;
using VRPhysicsLab.Physics;

namespace VRPhysicsLab.UI
{
    /// <summary>
    /// Formats and updates the digital telemetry dashboard for Station 2 (Friction Experiment).
    /// Displays live speed, stopping distance, deceleration, and derived friction coefficient comparison.
    /// </summary>
    public class FrictionDisplayUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private FrictionMeasurementSystem measurementSystem;
        [SerializeField] private SurfacePlateSlot surfaceSlot;
        [SerializeField] private FrictionCart cart;

        [Header("Status Header")]
        [SerializeField] private TMP_Text statusBadgeText;
        [SerializeField] private Image statusBadgeBg;
        [SerializeField] private TMP_Text surfaceBadgeText;

        [Header("Telemetry Cards")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private TMP_Text decelText;

        [Header("Friction Comparison")]
        [SerializeField] private TMP_Text expMuText;
        [SerializeField] private TMP_Text theoMuText;
        [SerializeField] private TMP_Text errorText;

        [Header("Bottom Banner")]
        [SerializeField] private TMP_Text hintText;

        [Header("Fallback Dashboard Text")]
        [SerializeField] private TMP_Text fallbackText;

        private void Start()
        {
            if (measurementSystem == null) measurementSystem = Object.FindFirstObjectByType<FrictionMeasurementSystem>();
            if (surfaceSlot == null) surfaceSlot = Object.FindFirstObjectByType<SurfacePlateSlot>();
            if (cart == null) cart = Object.FindFirstObjectByType<FrictionCart>();

            if (measurementSystem != null)
            {
                measurementSystem.onMeasurementUpdated.AddListener(UpdateDisplay);
                measurementSystem.onMeasurementCompleted.AddListener(OnCompleted);
            }

            if (surfaceSlot != null)
            {
                surfaceSlot.onSurfaceInstalled.AddListener(OnSurfaceChanged);
                surfaceSlot.onSurfaceRemoved.AddListener(OnSurfaceRemoved);
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

            float timeVal = measurementSystem != null ? measurementSystem.ElapsedTime : 0f;
            float distVal = measurementSystem != null ? measurementSystem.CurrentDistance : 0f;
            float speedVal = measurementSystem != null ? measurementSystem.CurrentSpeed : 0f;
            float decelVal = 0f;

            string sName = surfaceSlot != null && surfaceSlot.InstalledPlate != null
                ? surfaceSlot.InstalledPlate.DisplayName
                : "Plaka Tak\u0131l\u0131 De\u011fil";

            float theoMu = surfaceSlot != null && surfaceSlot.InstalledPlate != null
                ? surfaceSlot.InstalledPlate.FrictionCoefficient
                : 0f;

            float expMu = 0f;
            float relError = 0f;

            if (state == MeasurementState.Completed && measurementSystem != null)
            {
                var res = measurementSystem.LastResult;
                decelVal = res.deceleration;
                expMu = res.experimentalMu;
                theoMu = res.theoreticalMu;
                relError = res.relativeError;
                distVal = res.stoppingDistance;
                timeVal = res.stoppingTime;
            }
            else if (state == MeasurementState.Measuring && timeVal > 0.05f)
            {
                decelVal = (measurementSystem.InitialVelocity - speedVal) / timeVal;
            }

            // Status Badge Formatting
            string statusStr;
            Color badgeBgColor;
            string hintStr;

            switch (state)
            {
                case MeasurementState.Idle:
                    statusStr = "● HAZIR";
                    badgeBgColor = new Color(0.06f, 0.45f, 0.25f, 0.85f);
                    hintStr = surfaceSlot != null && surfaceSlot.HasSurfaceInstalled
                        ? "Fırlatmak için masadaki <color=#FF5555><b>KIRMIZI</b></color> butona basın."
                        : "Lütfen önce ray üzerindeki yuvaya bir <b>Yüzey Plakası</b> yerleştirin!";
                    break;
                case MeasurementState.Measuring:
                    statusStr = ">> HAREKET HALİNDE...";
                    badgeBgColor = new Color(0.05f, 0.35f, 0.65f, 0.85f);
                    hintStr = $"{sName} yüzeyinde sürtünme kuvveti cismi yavaşlatıyor...";
                    break;
                case MeasurementState.Completed:
                    statusStr = "● TAMAMLANDI";
                    badgeBgColor = new Color(0.65f, 0.45f, 0.05f, 0.85f);
                    hintStr = "Ölçüm bitti! Yeniden denemek için <color=#FFCC00><b>SARI</b></color> butona basın.";
                    break;
                default:
                    statusStr = "BEKLEN\u0130YOR";
                    badgeBgColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);
                    hintStr = "";
                    break;
            }

            if (statusBadgeText != null) statusBadgeText.text = statusStr;
            if (statusBadgeBg != null) statusBadgeBg.color = badgeBgColor;
            if (surfaceBadgeText != null) surfaceBadgeText.text = $"Y\u00fczey: <b>{sName}</b> (\u03bc={theoMu:F2})";

            if (timeText != null) timeText.text = $"{timeVal:F2} s";
            if (distanceText != null) distanceText.text = $"{distVal:F2} m";
            if (speedText != null) speedText.text = $"{speedVal:F2} m/s";
            if (decelText != null) decelText.text = $"{decelVal:F2} m/s\u00b2";

            if (expMuText != null) expMuText.text = state == MeasurementState.Completed ? $"{expMu:F3}" : "--";
            if (theoMuText != null) theoMuText.text = $"{theoMu:F3}";
            if (errorText != null) errorText.text = state == MeasurementState.Completed ? $"%{relError:F1}" : "-- %";

            if (hintText != null) hintText.text = hintStr;

            if (fallbackText != null)
            {
                fallbackText.text =
                    $"<size=18><color=#94A3B8>VR F\u0130Z\u0130K LAB</color> | <color=#FFFFFF>\u0130STASYON 2: S\u00dcRT\u00dcNME KUVVET\u0130</color></size>   <align=right><color=#00FFAA><b>{statusStr}</b></color></align>\n\n" +
                    $"Aktif Y\u00fczey: <b><color=#F59E0B>{sName}</color></b> (Teorik \u03bc = <b>{theoMu:F2}</b>)\n\n" +
                    $"<b><color=#38BDF8>CANLI \u00d6L\u00c7\u00dcMLER</color></b>                   <b><color=#A855F7>S\u00dcRT\u00dcNME ANAL\u0130Z\u0130</color></b>\n" +
                    $"Durma S\u00fcresi  : <b>{timeVal:F2} s</b>            Deneysel \u03bc  : <b>{(state == MeasurementState.Completed ? $"{expMu:F3}" : "--")}</b>\n" +
                    $"Durma Mesafesi: <b>{distVal:F2} m</b>            Teorik \u03bc    : <b>{theoMu:F3}</b>\n" +
                    $"Yava\u015flama \u0130vmesi: <b>{decelVal:F2} m/s\u00b2</b>          Hata Pay\u0131   : <b>{(state == MeasurementState.Completed ? $"%{relError:F1}" : "-- %")}</b>\n\n" +
                    $"<size=13><color=#94A3B8>{hintStr}</color></size>";
            }
        }

        private void OnCompleted(FrictionMeasurementResult res) => UpdateDisplay();
        private void OnSurfaceChanged(SurfacePlate plate) => UpdateDisplay();
        private void OnSurfaceRemoved() => UpdateDisplay();
    }
}
