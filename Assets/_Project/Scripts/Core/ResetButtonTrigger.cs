using UnityEngine;
using BNG;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Connects BNG.Button onButtonDown event to ResetManager.ResetAll().
    /// </summary>
    public class ResetButtonTrigger : MonoBehaviour
    {
        private void Start()
        {
            var btn = GetComponent<BNG.Button>();
            if (btn != null)
            {
                btn.onButtonDown.AddListener(OnButtonPressed);
            }
        }

        public void OnButtonPressed()
        {
            Debug.Log("[ResetButtonTrigger] Physical reset button pressed!");
            if (ResetManager.Instance != null)
            {
                ResetManager.Instance.ResetAll();
            }
        }
    }
}
