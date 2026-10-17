using UnityEngine;
using BNG;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Connects BNG.Button onButtonDown event to ForceApplicator.StartApplyingForce().
    /// </summary>
    public class ForceLauncherButtonTrigger : MonoBehaviour
    {
        [SerializeField] private ForceApplicator forceApplicator;

        private void Start()
        {
            if (forceApplicator == null)
            {
                forceApplicator = Object.FindFirstObjectByType<ForceApplicator>();
            }

            var btn = GetComponent<BNG.Button>();
            if (btn != null)
            {
                btn.onButtonDown.AddListener(OnButtonPressed);
            }
        }

        public void OnButtonPressed()
        {
            Debug.Log("[ForceLauncherButtonTrigger] Launch button pressed!");
            if (forceApplicator != null)
            {
                forceApplicator.StartApplyingForce();
            }
        }
    }
}
