using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Central manager that tracks and resets all IResetable objects in the lab.
    /// </summary>
    public class ResetManager : MonoBehaviour
    {
        public static ResetManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("If true, automatically searches and registers all IResetable components in the scene on Start.")]
        [SerializeField] private bool autoFindResetablesOnStart = true;

        [Header("Events")]
        public UnityEvent onBeforeReset;
        public UnityEvent onAfterReset;

        private readonly List<IResetable> resetables = new List<IResetable>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (autoFindResetablesOnStart)
            {
                RefreshResetablesList();
            }
        }

        /// <summary>
        /// Registers a resetable object to the manager.
        /// </summary>
        public void Register(IResetable resetable)
        {
            if (resetable != null && !resetables.Contains(resetable))
            {
                resetables.Add(resetable);
            }
        }

        /// <summary>
        /// Unregisters a resetable object from the manager.
        /// </summary>
        public void Unregister(IResetable resetable)
        {
            if (resetable != null && resetables.Contains(resetable))
            {
                resetables.Remove(resetable);
            }
        }

        /// <summary>
        /// Scans the scene and populates the list of resetables.
        /// </summary>
        public void RefreshResetablesList()
        {
            resetables.Clear();
            var found = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in found)
            {
                if (mb is IResetable resetable)
                {
                    resetables.Add(resetable);
                }
            }
        }

        /// <summary>
        /// Resets all registered objects back to their initial recorded states.
        /// </summary>
        [ContextMenu("Reset All")]
        public void ResetAll()
        {
            if (resetables == null || resetables.Count == 0)
            {
                RefreshResetablesList();
            }

            onBeforeReset?.Invoke();

            foreach (var resetable in resetables)
            {
                resetable?.ResetToInitialState();
            }

            onAfterReset?.Invoke();
            Debug.Log($"[ResetManager] Reset {resetables.Count} objects to their initial state.");
        }
    }
}
