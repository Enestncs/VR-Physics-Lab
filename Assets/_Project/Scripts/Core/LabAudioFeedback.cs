using System.Reflection;
using UnityEngine;
using BNG;
using VRPhysicsLab.Physics;
using VRPhysicsLab.Measurement;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Centralized procedural audio and VR haptic feedback manager for all 3 physics stations.
    /// Generates crisp, zero-dependency laboratory sound effects at runtime and auto-binds to
    /// buttons, launchers, photogates, ramps, mass/surface attachments, and measurement completions.
    /// </summary>
    public class LabAudioFeedback : MonoBehaviour
    {
        public static LabAudioFeedback Instance { get; private set; }

        [Header("Audio Volume")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 0.75f;

        [Header("Haptics")]
        [SerializeField] private bool enableHaptics = true;

        private AudioClip clipButtonClick;
        private AudioClip clipButtonRelease;
        private AudioClip clipLauncherFire;
        private AudioClip clipGateBeep;
        private AudioClip clipCompleteChime;
        private AudioClip clipAngleStep;
        private AudioClip clipSnapAttach;
        private AudioClip clipResetSweep;

        private bool suppressMassSound = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            GenerateProceduralClips();
        }

        private void Start()
        {
            BindAllLabEvents();
            // Prevent initial Awake/Start mass initialization from playing a snap sound
            Invoke(nameof(EnableMassSound), 0.35f);
        }

        private void EnableMassSound() => suppressMassSound = false;

        private void BindAllLabEvents()
        {
            // 1. Populate button click/release clips on every VRPhysicalButton that has none
            var buttons = Object.FindObjectsByType<VRPhysicalButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var clickField = typeof(VRPhysicalButton).GetField("clickSound", BindingFlags.NonPublic | BindingFlags.Instance);
            var releaseField = typeof(VRPhysicalButton).GetField("releaseSound", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                if (clickField != null && clickField.GetValue(btn) == null)
                    clickField.SetValue(btn, clipButtonClick);
                if (releaseField != null && releaseField.GetValue(btn) == null)
                    releaseField.SetValue(btn, clipButtonRelease);
            }

            // 2. Station 1: ForceApplicator, CartPhysics, GateSensors, MeasurementSystem
            var forceApp = Object.FindFirstObjectByType<ForceApplicator>();
            if (forceApp != null)
            {
                forceApp.onForceStarted.AddListener(() =>
                {
                    PlayAt(clipLauncherFire, forceApp.transform.position, 0.85f);
                    PulseHaptics(0.55f, 0.08f);
                });
            }

            var newtonCart = Object.FindFirstObjectByType<CartPhysics>();
            if (newtonCart != null)
            {
                newtonCart.onMassChanged.AddListener(_ =>
                {
                    if (!suppressMassSound)
                    {
                        PlayAt(clipSnapAttach, newtonCart.transform.position, 0.70f);
                        PulseHaptics(0.45f, 0.06f);
                    }
                });
            }

            var gates = Object.FindObjectsByType<GateSensor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var gate in gates)
            {
                if (gate == null) continue;
                var g = gate;
                g.onCartTriggered.AddListener((sensor, col) =>
                {
                    PlayAt(clipGateBeep, g.transform.position, 0.65f);
                });
            }

            var meas1 = Object.FindFirstObjectByType<MeasurementSystem>();
            if (meas1 != null)
            {
                meas1.onMeasurementCompleted.AddListener(_ =>
                {
                    PlayAt(clipCompleteChime, meas1.transform.position, 0.80f);
                    PulseHaptics(0.40f, 0.10f);
                });
            }

            // 3. Station 2: FrictionImpulseLauncher, SurfacePlateSlot, FrictionMeasurementSystem
            var frictionLauncher = Object.FindFirstObjectByType<FrictionImpulseLauncher>();
            if (frictionLauncher != null)
            {
                frictionLauncher.onLaunched.AddListener(() =>
                {
                    PlayAt(clipLauncherFire, frictionLauncher.transform.position, 0.85f);
                    PulseHaptics(0.55f, 0.08f);
                });
            }

            var surfaceSlot = Object.FindFirstObjectByType<SurfacePlateSlot>();
            if (surfaceSlot != null)
            {
                surfaceSlot.onSurfaceInstalled.AddListener(_ =>
                {
                    if (!suppressMassSound)
                    {
                        PlayAt(clipSnapAttach, surfaceSlot.transform.position, 0.72f);
                        PulseHaptics(0.45f, 0.06f);
                    }
                });
            }

            var meas2 = Object.FindFirstObjectByType<FrictionMeasurementSystem>();
            if (meas2 != null)
            {
                meas2.onMeasurementCompleted.AddListener(_ =>
                {
                    PlayAt(clipCompleteChime, meas2.transform.position, 0.80f);
                    PulseHaptics(0.40f, 0.10f);
                });
            }

            // 4. Station 3: AdjustableRamp, InclinedCart, InclinedMeasurementSystem
            var ramp = Object.FindFirstObjectByType<AdjustableRamp>();
            if (ramp != null)
            {
                ramp.onAngleChanged.AddListener(_ =>
                {
                    PlayAt(clipAngleStep, ramp.transform.position, 0.70f);
                    PulseHaptics(0.35f, 0.05f);
                });
            }

            var inclinedCart = Object.FindFirstObjectByType<InclinedCart>();
            if (inclinedCart != null)
            {
                inclinedCart.onReleased.AddListener(() =>
                {
                    PlayAt(clipLauncherFire, inclinedCart.transform.position, 0.65f);
                    PulseHaptics(0.40f, 0.06f);
                });
            }

            var meas3 = Object.FindFirstObjectByType<InclinedMeasurementSystem>();
            if (meas3 != null)
            {
                meas3.onMeasurementCompleted.AddListener(_ =>
                {
                    PlayAt(clipCompleteChime, meas3.transform.position, 0.80f);
                    PulseHaptics(0.40f, 0.10f);
                });
            }

            // 5. ResetManager
            var resetMgr = Object.FindFirstObjectByType<ResetManager>();
            if (resetMgr != null)
            {
                resetMgr.onBeforeReset.AddListener(() =>
                {
                    Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                    PlayAt(clipResetSweep, pos, 0.55f);
                });
            }
        }

        public void PlayAt(AudioClip clip, Vector3 worldPos, float volumeScale = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, worldPos, Mathf.Clamp01(masterVolume * volumeScale));
        }

        private void PulseHaptics(float amplitude, float duration)
        {
            if (!enableHaptics || InputBridge.Instance == null) return;
            InputBridge.Instance.VibrateController(40f, amplitude, duration, ControllerHand.Right);
            InputBridge.Instance.VibrateController(40f, amplitude * 0.7f, duration, ControllerHand.Left);
        }

        #region Procedural Audio Synthesis

        private void GenerateProceduralClips()
        {
            clipButtonClick = SynthesizeClick("SFX_ButtonClick", 0.038f, 920f, 260f, 0.75f);
            clipButtonRelease = SynthesizeClick("SFX_ButtonRelease", 0.028f, 640f, 220f, 0.45f);
            clipLauncherFire = SynthesizeLauncher("SFX_LauncherFire", 0.18f);
            clipGateBeep = SynthesizeTone("SFX_GateBeep", 0.075f, 1320f, 1320f, 0.55f);
            clipCompleteChime = SynthesizeTwoToneChime("SFX_CompleteChime", 0.24f, 1046.5f, 1318.5f);
            clipAngleStep = SynthesizeClick("SFX_AngleStep", 0.055f, 520f, 780f, 0.60f);
            clipSnapAttach = SynthesizeClick("SFX_SnapAttach", 0.070f, 440f, 880f, 0.65f);
            clipResetSweep = SynthesizeTone("SFX_ResetSweep", 0.14f, 660f, 330f, 0.45f);
        }

        private static AudioClip SynthesizeClick(string name, float duration, float startFreq, float endFreq, float gain)
        {
            int sr = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float env = Mathf.Pow(1f - t, 3.2f);
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * freq / sr;
                float tone = Mathf.Sin(phase);
                float transient = (t < 0.08f) ? (Random.Range(-1f, 1f) * 0.35f) : 0f;
                data[i] = Mathf.Clamp((tone + transient) * env * gain, -1f, 1f);
            }
            var clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthesizeLauncher(string name, float duration)
        {
            int sr = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float punchEnv = Mathf.Pow(1f - t, 2.4f);
                float freq = Mathf.Lerp(210f, 55f, Mathf.Sqrt(t));
                phase += 2f * Mathf.PI * freq / sr;
                float thump = Mathf.Sin(phase) * punchEnv * 0.75f;
                float whoosh = Random.Range(-1f, 1f) * Mathf.Sin(t * Mathf.PI) * (1f - t) * 0.25f;
                data[i] = Mathf.Clamp(thump + whoosh, -1f, 1f);
            }
            var clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthesizeTone(string name, float duration, float startFreq, float endFreq, float gain)
        {
            int sr = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float attack = Mathf.Clamp01(t / 0.08f);
                float release = Mathf.Clamp01((1f - t) / 0.35f);
                float env = attack * release;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * freq / sr;
                data[i] = Mathf.Sin(phase) * env * gain;
            }
            var clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthesizeTwoToneChime(string name, float duration, float freq1, float freq2)
        {
            int sr = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                bool second = t >= 0.42f;
                float localT = second ? (t - 0.42f) / 0.58f : t / 0.42f;
                float env = Mathf.Clamp01(localT / 0.06f) * Mathf.Pow(1f - localT, 1.5f);
                float freq = second ? freq2 : freq1;
                phase += 2f * Mathf.PI * freq / sr;
                data[i] = (Mathf.Sin(phase) * 0.75f + Mathf.Sin(phase * 2f) * 0.2f) * env * 0.55f;
            }
            var clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion
    }
}
