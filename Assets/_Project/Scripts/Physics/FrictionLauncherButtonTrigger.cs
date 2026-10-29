using UnityEngine;
using BNG;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Connects VR button presses (physical push, BNG.Button, VRPhysicalButton)
    /// to FrictionImpulseLauncher.Launch().
    /// </summary>
    public class FrictionLauncherButtonTrigger : MonoBehaviour
    {
        [SerializeField] private FrictionImpulseLauncher launcher;

        private void Start()
        {
            if (launcher == null)
            {
                launcher = Object.FindFirstObjectByType<FrictionImpulseLauncher>();
            }

            var bngBtn = GetComponent<BNG.Button>();
            if (bngBtn != null)
            {
                bngBtn.onButtonDown.AddListener(OnButtonPressed);
            }

            var physBtn = GetComponent<VRPhysicalButton>();
            if (physBtn != null)
            {
                physBtn.onPressed.AddListener(OnButtonPressed);
            }
        }

        public void OnButtonPressed()
        {
            Debug.Log("[FrictionLauncherButtonTrigger] Launch button triggered!");
            if (launcher != null)
            {
                launcher.Launch();
            }
        }
    }
}
