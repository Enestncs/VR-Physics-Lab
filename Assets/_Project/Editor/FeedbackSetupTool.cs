#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRPhysicsLab.Core;
using VRPhysicsLab.Physics;
using VRPhysicsLab.Visuals;

namespace VRPhysicsLab.Editor
{
    public static class FeedbackSetupTool
    {
        [MenuItem("VR Physics Lab/Setup Visual Vectors & Audio Feedback", priority = 105)]
        public static void SetupFeedbackSystems()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            Undo.SetCurrentGroupName("Setup Visual Vectors & Audio Feedback");
            int undoGroup = Undo.GetCurrentGroup();

            int attachedVisualizers = 0;

            // 1. Station 1 Cart
            var newtonCart = Object.FindFirstObjectByType<CartPhysics>();
            if (newtonCart != null)
            {
                var vis = newtonCart.GetComponent<ForceVectorVisualizer>();
                if (vis == null) vis = Undo.AddComponent<ForceVectorVisualizer>(newtonCart.gameObject);
                var so = new SerializedObject(vis);
                so.FindProperty("stationType").enumValueIndex = (int)VectorStationType.Station1_Newton;
                so.ApplyModifiedProperties();
                attachedVisualizers++;
            }

            // 2. Station 2 Cart
            var frictionCart = Object.FindFirstObjectByType<FrictionCart>();
            if (frictionCart != null)
            {
                var vis = frictionCart.GetComponent<ForceVectorVisualizer>();
                if (vis == null) vis = Undo.AddComponent<ForceVectorVisualizer>(frictionCart.gameObject);
                var so = new SerializedObject(vis);
                so.FindProperty("stationType").enumValueIndex = (int)VectorStationType.Station2_Friction;
                so.ApplyModifiedProperties();
                attachedVisualizers++;
            }

            // 3. Station 3 Cart
            var inclinedCart = Object.FindFirstObjectByType<InclinedCart>();
            if (inclinedCart != null)
            {
                var vis = inclinedCart.GetComponent<ForceVectorVisualizer>();
                if (vis == null) vis = Undo.AddComponent<ForceVectorVisualizer>(inclinedCart.gameObject);
                var so = new SerializedObject(vis);
                so.FindProperty("stationType").enumValueIndex = (int)VectorStationType.Station3_InclinedPlane;
                so.ApplyModifiedProperties();
                attachedVisualizers++;
            }

            // 4. LabAudioFeedback & DesktopTestController on Managers
            var managersGo = GameObject.Find("Managers") ?? Object.FindFirstObjectByType<ResetManager>()?.gameObject;
            if (managersGo == null)
            {
                managersGo = new GameObject("Managers");
                Undo.RegisterCreatedObjectUndo(managersGo, "Create Managers");
            }
            if (Object.FindFirstObjectByType<LabAudioFeedback>() == null)
            {
                Undo.AddComponent<LabAudioFeedback>(managersGo);
            }
            if (Object.FindFirstObjectByType<DesktopTestController>() == null)
            {
                Undo.AddComponent<DesktopTestController>(managersGo);
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log($"[FeedbackSetupTool] Configured ForceVectorVisualizer on {attachedVisualizers} carts and ensured LabAudioFeedback in scene.");
        }
    }
}
#endif
