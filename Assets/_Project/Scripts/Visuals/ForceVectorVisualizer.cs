using UnityEngine;
using VRPhysicsLab.Physics;
using VRPhysicsLab.Measurement;

namespace VRPhysicsLab.Visuals
{
    public enum VectorStationType
    {
        AutoDetect,
        Station1_Newton,
        Station2_Friction,
        Station3_InclinedPlane
    }

    /// <summary>
    /// Renders live 3D force, acceleration, and velocity vector arrows above any station's cart.
    /// Automatically updates during Idle preview (so students see vectors react immediately when adding
    /// weights, swapping surface plates, or tilting the ramp) as well as during active motion.
    /// </summary>
    public class ForceVectorVisualizer : MonoBehaviour
    {
        [Header("Station Mode")]
        [SerializeField] private VectorStationType stationType = VectorStationType.AutoDetect;
        [SerializeField] private bool showVectors = true;

        [Header("Visual Scaling")]
        [Tooltip("Meters of arrow length per Newton of force.")]
        [SerializeField] private float forceScale = 0.042f;
        [Tooltip("Meters of arrow length per m/s² of acceleration.")]
        [SerializeField] private float accelScale = 0.048f;
        [Tooltip("Meters of arrow length per m/s of velocity.")]
        [SerializeField] private float velocityScale = 0.14f;
        [SerializeField] private float minArrowLength = 0.08f;
        [SerializeField] private float maxArrowLength = 0.38f;

        // Station references (auto-resolved at Start)
        private CartPhysics newtonCart;
        private ForceApplicator forceApplicator;
        private MeasurementSystem newtonMeas;

        private FrictionCart frictionCart;
        private SurfacePlateSlot surfaceSlot;
        private FrictionMeasurementSystem frictionMeas;

        private InclinedCart inclinedCart;
        private AdjustableRamp adjustableRamp;
        private InclinedMeasurementSystem inclinedMeas;

        // Spawned 3D arrows
        private Transform arrowsRoot;
        private VectorArrow3D primaryForceArrow;   // F_net (St1), f_s (St2), mg·sinθ (St3)
        private VectorArrow3D secondaryArrow;      // a (St1), v (St2), a (St3)
        private VectorArrow3D normalForceArrow;    // N = mg·cosθ (St3)

        private static readonly Color ColorForceRed = new Color(0.95f, 0.28f, 0.25f, 1f);
        private static readonly Color ColorFrictionAmber = new Color(0.98f, 0.62f, 0.12f, 1f);
        private static readonly Color ColorAccelCyan = new Color(0.22f, 0.78f, 0.98f, 1f);
        private static readonly Color ColorNormalGreen = new Color(0.20f, 0.84f, 0.45f, 1f);

        public bool ShowVectors
        {
            get => showVectors;
            set
            {
                showVectors = value;
                if (arrowsRoot != null) arrowsRoot.gameObject.SetActive(showVectors);
            }
        }

        private void Start()
        {
            ResolveStationReferences();
            CreateArrows();
        }

        private void ResolveStationReferences()
        {
            newtonCart = GetComponent<CartPhysics>();
            frictionCart = GetComponent<FrictionCart>();
            inclinedCart = GetComponent<InclinedCart>();

            if (stationType == VectorStationType.AutoDetect)
            {
                if (newtonCart != null) stationType = VectorStationType.Station1_Newton;
                else if (frictionCart != null) stationType = VectorStationType.Station2_Friction;
                else if (inclinedCart != null) stationType = VectorStationType.Station3_InclinedPlane;
            }

            switch (stationType)
            {
                case VectorStationType.Station1_Newton:
                    forceApplicator = Object.FindFirstObjectByType<ForceApplicator>();
                    newtonMeas = Object.FindFirstObjectByType<MeasurementSystem>();
                    break;
                case VectorStationType.Station2_Friction:
                    surfaceSlot = Object.FindFirstObjectByType<SurfacePlateSlot>();
                    frictionMeas = Object.FindFirstObjectByType<FrictionMeasurementSystem>();
                    break;
                case VectorStationType.Station3_InclinedPlane:
                    adjustableRamp = inclinedCart != null && inclinedCart.Ramp != null
                        ? inclinedCart.Ramp
                        : Object.FindFirstObjectByType<AdjustableRamp>();
                    inclinedMeas = Object.FindFirstObjectByType<InclinedMeasurementSystem>();
                    break;
            }
        }

        private void CreateArrows()
        {
            var rootGo = new GameObject("ForceVectors_Root");
            // Keep arrows in world space (not parented to cart) to avoid cart scale distortion
            arrowsRoot = rootGo.transform;

            switch (stationType)
            {
                case VectorStationType.Station1_Newton:
                    primaryForceArrow = SpawnArrow("Arrow_Fnet", ColorForceRed);
                    secondaryArrow = SpawnArrow("Arrow_Accel", ColorAccelCyan);
                    break;

                case VectorStationType.Station2_Friction:
                    primaryForceArrow = SpawnArrow("Arrow_Friction", ColorFrictionAmber);
                    secondaryArrow = SpawnArrow("Arrow_Velocity", ColorAccelCyan);
                    break;

                case VectorStationType.Station3_InclinedPlane:
                    primaryForceArrow = SpawnArrow("Arrow_MgSinTheta", ColorForceRed);
                    secondaryArrow = SpawnArrow("Arrow_Accel", ColorAccelCyan);
                    normalForceArrow = SpawnArrow("Arrow_Normal", ColorNormalGreen);
                    break;
            }

            if (arrowsRoot != null) arrowsRoot.gameObject.SetActive(showVectors);
        }

        private VectorArrow3D SpawnArrow(string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(arrowsRoot, false);
            var arrow = go.AddComponent<VectorArrow3D>();
            arrow.Initialize(name, color);
            return arrow;
        }

        private void LateUpdate()
        {
            if (!showVectors || arrowsRoot == null) return;

            switch (stationType)
            {
                case VectorStationType.Station1_Newton:
                    UpdateStation1Vectors();
                    break;
                case VectorStationType.Station2_Friction:
                    UpdateStation2Vectors();
                    break;
                case VectorStationType.Station3_InclinedPlane:
                    UpdateStation3Vectors();
                    break;
            }
        }

        private void UpdateStation1Vectors()
        {
            if (newtonCart == null) return;

            bool completed = newtonMeas != null && newtonMeas.State == MeasurementState.Completed;
            float netForce = forceApplicator != null ? forceApplicator.NetForce : 4.0f;
            float mass = Mathf.Max(0.1f, newtonCart.TotalMass);
            float accel = netForce / mass;

            // Hide vectors once cart stops at finish; show both during Idle preview and active force application
            bool active = !completed && (forceApplicator == null || forceApplicator.IsApplyingForce || !forceApplicator.HasLaunched);

            Vector3 cartPos = newtonCart.transform.position;
            // Offset higher when weights are stacked on the cart
            float baseY = newtonCart.AttachedWeightCount > 0 ? 0.20f : 0.14f;

            Vector3 forceOrigin = cartPos + new Vector3(0.08f, baseY, 0f);
            float forceLen = ClampLen(netForce * forceScale);
            primaryForceArrow?.SetVector(forceOrigin, Vector3.right, forceLen, $"F = {netForce:F1} N", active, 0.036f);

            Vector3 accelOrigin = cartPos + new Vector3(0.08f, baseY + 0.085f, 0f);
            float accelLen = ClampLen(accel * accelScale);
            secondaryArrow?.SetVector(accelOrigin, Vector3.right, accelLen, $"a = {accel:F2} m/s²", active, 0.036f);
        }

        private void UpdateStation2Vectors()
        {
            if (frictionCart == null) return;

            float mu = 0.05f;
            if (surfaceSlot != null && surfaceSlot.InstalledPlate != null)
            {
                mu = surfaceSlot.InstalledPlate.FrictionCoefficient;
            }

            float mass = frictionCart.Mass;
            float frictionForce = mu * mass * 9.81f;
            float speed = frictionCart.CurrentSpeed;

            bool isMeasuring = frictionMeas != null && frictionMeas.State == MeasurementState.Measuring;
            bool isIdle = frictionMeas == null || frictionMeas.State == MeasurementState.Idle;
            bool showFriction = isIdle || (isMeasuring && speed > 0.01f);
            bool showVelocity = isMeasuring && speed > 0.03f;

            Vector3 cartPos = frictionCart.transform.position;

            // Friction opposes motion (-X)
            Vector3 fOrigin = cartPos + new Vector3(-0.08f, 0.14f, 0f);
            float fLen = ClampLen(frictionForce * forceScale);
            primaryForceArrow?.SetVector(fOrigin, Vector3.left, fLen, $"f_s = {frictionForce:F2} N", showFriction, 0.040f);

            // Live velocity along +X during slide
            Vector3 vOrigin = cartPos + new Vector3(0.08f, 0.14f, 0f);
            float vLen = ClampLen(speed * velocityScale);
            secondaryArrow?.SetVector(vOrigin, Vector3.right, vLen, $"v = {speed:F2} m/s", showVelocity, 0.040f);
        }

        private void UpdateStation3Vectors()
        {
            if (inclinedCart == null || adjustableRamp == null || adjustableRamp.RampPivot == null) return;

            Transform pivot = adjustableRamp.RampPivot;
            float rad = adjustableRamp.AngleRadians;
            float m = inclinedCart.Mass;
            float g = inclinedCart.Gravity;

            float parallelForce = m * g * Mathf.Sin(rad);
            float normalForce = m * g * Mathf.Cos(rad);
            float accel = g * Mathf.Sin(rad);

            bool active = !inclinedCart.HasFinished;

            // Ramp local +X is downhill along the incline; local +Y is perpendicular (normal) to the ramp
            Vector3 downhillDir = pivot.right.normalized;
            Vector3 normalDir = pivot.up.normalized;

            Vector3 cartCenter = inclinedCart.transform.position + normalDir * 0.09f;

            // mg·sin(θ) along ramp
            Vector3 fOrigin = cartCenter + downhillDir * 0.08f;
            float fLen = ClampLen(parallelForce * forceScale);
            primaryForceArrow?.SetVector(fOrigin, downhillDir, fLen, $"mg·sinθ = {parallelForce:F2} N", active, 0.042f);

            // Acceleration a = g·sin(θ) slightly above mg·sin(θ)
            Vector3 aOrigin = cartCenter + normalDir * 0.075f + downhillDir * 0.08f;
            float aLen = ClampLen(accel * accelScale);
            secondaryArrow?.SetVector(aOrigin, downhillDir, aLen, $"a = {accel:F2} m/s²", active, 0.042f);

            // Normal force N = mg·cos(θ) perpendicular to ramp
            Vector3 nOrigin = cartCenter + normalDir * 0.03f;
            float nLen = ClampLen(normalForce * (forceScale * 0.65f));
            normalForceArrow?.SetVector(nOrigin, normalDir, nLen, $"N = {normalForce:F2} N", active, 0.036f);
        }

        private float ClampLen(float rawLength)
        {
            if (rawLength <= 0.001f) return 0f;
            return Mathf.Clamp(rawLength, minArrowLength, maxArrowLength);
        }

        private void OnDestroy()
        {
            if (arrowsRoot != null)
            {
                Destroy(arrowsRoot.gameObject);
            }
        }
    }
}
