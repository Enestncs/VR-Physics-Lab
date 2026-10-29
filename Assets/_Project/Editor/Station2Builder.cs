#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;
using VRPhysicsLab.Core;
using VRPhysicsLab.Physics;
using VRPhysicsLab.Measurement;
using VRPhysicsLab.UI;
using BNG;

namespace VRPhysicsLab.Editor
{
    public static class Station2Builder
    {
        [MenuItem("VR Physics Lab/Build Station 2 (Friction Experiment)", priority = 100)]
        public static void BuildStation2()
        {
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            Undo.SetCurrentGroupName("Build Station 2");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Remove existing Station 2 root if present for clean idempotent build
            const string station2RootName = "--- [EXPERIMENT 2 - FRICTION] ---";
            var existingRoot = GameObject.Find(station2RootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
                Debug.Log($"[Station2Builder] Removed old {station2RootName} for clean rebuild.");
            }

            // 2. Load Materials & PhysicsMaterials
            var matTable = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Table.mat");
            var matTrack = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Track.mat");
            var matCart = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Cart.mat");
            var matWeight = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Weight.mat");
            var matBtnHousing = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_ButtonHousing.mat");
            var matBtnRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_ButtonRed.mat");
            var matBtnYellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_ButtonYellow.mat");
            var matIce = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_SurfaceIce.mat");
            var matWood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_SurfaceWood.mat");
            var matRubber = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_SurfaceRubber.mat");

            var pmatFrictionless = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Project/Materials/PhysicMat_Frictionless.physicMaterial");
            var pmatIce = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Project/Materials/PhysicMat_Ice.physicMaterial");
            var pmatWood = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Project/Materials/PhysicMat_Wood.physicMaterial");
            var pmatRubber = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Project/Materials/PhysicMat_Rubber.physicMaterial");

            var sndClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/BNG Framework/Sounds/click.wav");
            var sndRelease = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/BNG Framework/Sounds/click2.wav");

            // Root GameObject for Station 2
            var root = new GameObject(station2RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Station 2 Root");
            root.transform.position = new Vector3(2.4f, 0f, 0f);
            root.transform.rotation = Quaternion.identity;

            // 3. Station 2 Table
            var table = new GameObject("LabTable_Station2");
            table.transform.SetParent(root.transform, false);
            table.transform.localPosition = Vector3.zero;

            // TableTop
            var tabletop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tabletop.name = "TableTop";
            tabletop.transform.SetParent(table.transform, false);
            tabletop.transform.localPosition = new Vector3(0f, 0.88f, 1.10f);
            tabletop.transform.localScale = new Vector3(2.2f, 0.05f, 0.90f);
            if (matTable != null) tabletop.GetComponent<Renderer>().sharedMaterial = matTable;

            // Table Legs
            Vector3[] legPositions = new Vector3[]
            {
                new Vector3(-1.00f, 0.43f, 0.70f),
                new Vector3( 1.00f, 0.43f, 0.70f),
                new Vector3(-1.00f, 0.43f, 1.45f),
                new Vector3( 1.00f, 0.43f, 1.45f)
            };
            string[] legNames = new string[] { "Leg_FL", "Leg_FR", "Leg_BL", "Leg_BR" };
            for (int i = 0; i < 4; i++)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = legNames[i];
                leg.transform.SetParent(table.transform, false);
                leg.transform.localPosition = legPositions[i];
                leg.transform.localScale = new Vector3(0.08f, 0.86f, 0.08f);
                if (matTable != null) leg.GetComponent<Renderer>().sharedMaterial = matTable;
            }

            // 4. Station Signboard / Table Header
            var signPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signPost.name = "StationSign_Post";
            signPost.transform.SetParent(table.transform, false);
            signPost.transform.localPosition = new Vector3(-1.02f, 1.15f, 0.70f);
            signPost.transform.localScale = new Vector3(0.04f, 0.50f, 0.04f);
            if (matBtnHousing != null) signPost.GetComponent<Renderer>().sharedMaterial = matBtnHousing;

            var signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = "StationSign_Board";
            signBoard.transform.SetParent(signPost.transform, false);
            signBoard.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            signBoard.transform.localScale = new Vector3(0.05f, 0.22f, 0.45f);
            if (matTable != null) signBoard.GetComponent<Renderer>().sharedMaterial = matTable;

            var signCanvasGo = new GameObject("SignCanvas");
            signCanvasGo.transform.SetParent(signBoard.transform, false);
            signCanvasGo.transform.localPosition = new Vector3(0.55f, 0f, 0f);
            signCanvasGo.transform.localRotation = Quaternion.Euler(0, 90, 0);
            signCanvasGo.transform.localScale = Vector3.one * 0.002f;
            var signText = signCanvasGo.AddComponent<TextMeshProUGUI>();
            signText.text = "<size=24><b><color=#38BDF8>İSTASYON 2</color></b></size>\n<size=18>Sürtünme Kuvveti Deneyi</size>\n<size=12><color=#94A3B8>F_s = µ · m · g</color></size>";
            signText.alignment = TextAlignmentOptions.Center;
            signText.rectTransform.sizeDelta = new Vector2(220, 100);

            // 5. Track Assembly
            var trackAssembly = new GameObject("Track_Assembly_Friction");
            trackAssembly.transform.SetParent(root.transform, false);
            trackAssembly.transform.localPosition = Vector3.zero;

            // Track Base
            var trackBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trackBase.name = "Track_Base";
            trackBase.transform.SetParent(trackAssembly.transform, false);
            trackBase.transform.localPosition = new Vector3(0f, 0.92f, 1.10f);
            trackBase.transform.localScale = new Vector3(1.80f, 0.03f, 0.24f);
            if (matTrack != null) trackBase.GetComponent<Renderer>().sharedMaterial = matTrack;
            var trackBaseCol = trackBase.GetComponent<BoxCollider>();

            // Thin track surface visual overlay that changes color with plate
            var trackSurfaceVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trackSurfaceVisual.name = "Track_SurfaceVisual";
            trackSurfaceVisual.transform.SetParent(trackAssembly.transform, false);
            trackSurfaceVisual.transform.localPosition = new Vector3(0f, 0.936f, 1.10f);
            trackSurfaceVisual.transform.localScale = new Vector3(1.78f, 0.002f, 0.218f);
            Object.DestroyImmediate(trackSurfaceVisual.GetComponent<Collider>());
            var trackSurfaceRenderer = trackSurfaceVisual.GetComponent<Renderer>();
            if (matTrack != null) trackSurfaceRenderer.sharedMaterial = matTrack;

            // Rails
            var railBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            railBack.name = "Track_RailBack";
            railBack.transform.SetParent(trackAssembly.transform, false);
            railBack.transform.localPosition = new Vector3(0f, 0.945f, 1.22f);
            railBack.transform.localScale = new Vector3(1.80f, 0.04f, 0.02f);
            if (matTrack != null) railBack.GetComponent<Renderer>().sharedMaterial = matTrack;

            var railFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            railFront.name = "Track_RailFront";
            railFront.transform.SetParent(trackAssembly.transform, false);
            railFront.transform.localPosition = new Vector3(0f, 0.945f, 0.98f);
            railFront.transform.localScale = new Vector3(1.80f, 0.04f, 0.02f);
            if (matTrack != null) railFront.GetComponent<Renderer>().sharedMaterial = matTrack;

            // End Bumper
            var endBumper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            endBumper.name = "Track_EndBumper";
            endBumper.transform.SetParent(trackAssembly.transform, false);
            endBumper.transform.localPosition = new Vector3(0.89f, 0.955f, 1.10f);
            endBumper.transform.localScale = new Vector3(0.04f, 0.06f, 0.22f);
            if (matBtnHousing != null) endBumper.GetComponent<Renderer>().sharedMaterial = matBtnHousing;

            // Distance Mark Rulers along track
            var distanceMarksRoot = new GameObject("Distance_Marks");
            distanceMarksRoot.transform.SetParent(trackAssembly.transform, false);
            distanceMarksRoot.transform.localPosition = Vector3.zero;

            float startX = -0.68f;
            float[] distances = new float[] { 0.00f, 0.25f, 0.50f, 0.75f, 1.00f, 1.25f, 1.45f };
            foreach (float d in distances)
            {
                float xPos = startX + d;
                var tick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tick.name = $"Tick_{d:F2}m";
                tick.transform.SetParent(distanceMarksRoot.transform, false);
                tick.transform.localPosition = new Vector3(xPos, 0.94f, 0.98f);
                tick.transform.localScale = new Vector3(0.008f, 0.035f, 0.025f);
                if (matWeight != null) tick.GetComponent<Renderer>().sharedMaterial = matWeight;
                Object.DestroyImmediate(tick.GetComponent<Collider>());

                // Label on the front rail face
                var lblGo = new GameObject($"Label_{d:F2}m");
                lblGo.transform.SetParent(tick.transform, false);
                lblGo.transform.localPosition = new Vector3(0f, 0f, -0.6f);
                lblGo.transform.localRotation = Quaternion.Euler(0, 0, 0);
                lblGo.transform.localScale = Vector3.one * 0.015f;
                var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
                lblTmp.text = $"{d:F2}m";
                lblTmp.fontSize = 10;
                lblTmp.alignment = TextAlignmentOptions.Center;
                lblTmp.rectTransform.sizeDelta = new Vector2(50, 20);
            }

            // 6. SurfacePlateSlot (snaps plate, applies physics & visual materials)
            var slotGo = new GameObject("SurfacePlateSlot");
            slotGo.transform.SetParent(trackAssembly.transform, false);
            slotGo.transform.localPosition = new Vector3(0f, 0.936f, 1.10f);
            var slotCol = slotGo.AddComponent<BoxCollider>();
            slotCol.isTrigger = true;
            slotCol.center = new Vector3(0f, 0.08f, 0f);
            slotCol.size = new Vector3(1.70f, 0.20f, 0.26f);

            var slot = slotGo.AddComponent<SurfacePlateSlot>();
            // Use SerializedObject to set private serialized fields cleanly
            var slotSo = new SerializedObject(slot);
            slotSo.FindProperty("trackSurfaceCollider").objectReferenceValue = trackBaseCol;
            slotSo.FindProperty("trackSurfaceRenderer").objectReferenceValue = trackSurfaceRenderer;
            slotSo.FindProperty("defaultVisualMaterial").objectReferenceValue = matTrack;
            slotSo.FindProperty("defaultPhysicsMaterial").objectReferenceValue = pmatFrictionless;
            slotSo.FindProperty("snapTransform").objectReferenceValue = slotGo.transform;
            slotSo.ApplyModifiedProperties();

            // 7. Interchangeable Surface Plates & Plate Rack
            var plateStation = new GameObject("Surface_Station");
            plateStation.transform.SetParent(root.transform, false);
            plateStation.transform.localPosition = Vector3.zero;

            // Plate Rack Tray on the front edge of the table
            var tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "SurfacePlate_Tray";
            tray.transform.SetParent(plateStation.transform, false);
            tray.transform.localPosition = new Vector3(0.00f, 0.908f, 0.78f);
            tray.transform.localScale = new Vector3(0.80f, 0.02f, 0.22f);
            if (matBtnHousing != null) tray.GetComponent<Renderer>().sharedMaterial = matBtnHousing;

            // Tray Label
            var trayLabelGo = new GameObject("Tray_Label");
            trayLabelGo.transform.SetParent(tray.transform, false);
            trayLabelGo.transform.localPosition = new Vector3(0f, 0.6f, -0.48f);
            trayLabelGo.transform.localRotation = Quaternion.Euler(90, 0, 0);
            trayLabelGo.transform.localScale = Vector3.one * 0.003f;
            var trayLabel = trayLabelGo.AddComponent<TextMeshProUGUI>();
            trayLabel.text = "<size=18><b>DEĞİŞTİRİLEBİLİR YÜZEY PLAKALARI (SURFACE PLATES)</b></size>\n<size=14><color=#94A3B8>Plakayı tutup raya bırakın / Grab and drop onto track</color></size>";
            trayLabel.alignment = TextAlignmentOptions.Center;
            trayLabel.rectTransform.sizeDelta = new Vector2(250, 40);

            // Helper to build a grabbable surface plate cartridge
            System.Func<string, SurfaceType, string, float, Material, PhysicsMaterial, Color, Vector3, SurfacePlate> createPlate =
                (name, type, displayName, mu, visualMat, physMat, col, localPos) =>
            {
                var pGo = new GameObject(name);
                pGo.transform.SetParent(plateStation.transform, false);
                pGo.transform.localPosition = localPos;

                // Plate Base
                var pMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pMesh.name = "Plate_Base";
                pMesh.transform.SetParent(pGo.transform, false);
                pMesh.transform.localPosition = Vector3.zero;
                pMesh.transform.localScale = new Vector3(0.24f, 0.025f, 0.16f);
                if (visualMat != null) pMesh.GetComponent<Renderer>().sharedMaterial = visualMat;
                var pCol = pMesh.GetComponent<BoxCollider>();

                // Plate Grab Handle on top
                var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                handle.name = "Handle";
                handle.transform.SetParent(pGo.transform, false);
                handle.transform.localPosition = new Vector3(0f, 0.035f, 0f);
                handle.transform.localScale = new Vector3(0.12f, 0.03f, 0.04f);
                if (matBtnHousing != null) handle.GetComponent<Renderer>().sharedMaterial = matBtnHousing;
                Object.DestroyImmediate(handle.GetComponent<Collider>());

                // Plate Text on Handle
                var pTxtGo = new GameObject("Handle_Text");
                pTxtGo.transform.SetParent(handle.transform, false);
                pTxtGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                pTxtGo.transform.localRotation = Quaternion.Euler(90, 0, 0);
                pTxtGo.transform.localScale = Vector3.one * 0.004f;
                var pTxt = pTxtGo.AddComponent<TextMeshProUGUI>();
                pTxt.text = $"<b>{displayName}</b>\nµ = {mu:F2}";
                pTxt.alignment = TextAlignmentOptions.Center;
                pTxt.fontSize = 11;
                pTxt.rectTransform.sizeDelta = new Vector2(30, 20);

                // Rigidbody
                var rb = pGo.AddComponent<Rigidbody>();
                rb.mass = 0.8f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Grabbable (BNG)
                var grab = pGo.AddComponent<Grabbable>();
                grab.GrabPhysics = GrabPhysics.Velocity;

                // SurfacePlate script
                var sp = pGo.AddComponent<SurfacePlate>();
                var spSo = new SerializedObject(sp);
                spSo.FindProperty("surfaceType").enumValueIndex = (int)type;
                spSo.FindProperty("surfaceDisplayName").stringValue = displayName;
                spSo.FindProperty("frictionCoefficient").floatValue = mu;
                spSo.FindProperty("surfacePhysicsMaterial").objectReferenceValue = physMat;
                spSo.FindProperty("plateMaterial").objectReferenceValue = visualMat;
                spSo.FindProperty("plateColor").colorValue = col;
                spSo.ApplyModifiedProperties();

                // ResetableObject
                var resObj = pGo.AddComponent<ResetableObject>();

                return sp;
            };

            var plateIce = createPlate("SurfacePlate_Ice", SurfaceType.Ice, "Buz / Teflon", 0.05f, matIce, pmatIce, new Color(0.6f, 0.9f, 1f), new Vector3(-0.25f, 0.94f, 0.78f));
            var plateWood = createPlate("SurfacePlate_Wood", SurfaceType.Wood, "Ahşap / Zımpara", 0.20f, matWood, pmatWood, new Color(0.85f, 0.65f, 0.4f), new Vector3(0.00f, 0.94f, 0.78f));
            var plateRubber = createPlate("SurfacePlate_Rubber", SurfaceType.Rubber, "Kauçuk / Lastik", 0.55f, matRubber, pmatRubber, new Color(0.25f, 0.25f, 0.28f), new Vector3(0.25f, 0.94f, 0.78f));

            // 8. Friction Cart
            var cartGo = new GameObject("FrictionCart");
            cartGo.transform.SetParent(root.transform, false);
            cartGo.transform.localPosition = new Vector3(-0.68f, 0.965f, 1.10f);

            var cartBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cartBody.name = "Cart_Body";
            cartBody.transform.SetParent(cartGo.transform, false);
            cartBody.transform.localPosition = Vector3.zero;
            cartBody.transform.localScale = new Vector3(0.18f, 0.06f, 0.16f);
            if (matCart != null) cartBody.GetComponent<Renderer>().sharedMaterial = matCart;

            // Slider Contact Plate on Bottom
            var cartSkid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cartSkid.name = "Cart_ContactSkid";
            cartSkid.transform.SetParent(cartGo.transform, false);
            cartSkid.transform.localPosition = new Vector3(0f, -0.032f, 0f);
            cartSkid.transform.localScale = new Vector3(0.17f, 0.005f, 0.15f);
            if (matBtnHousing != null) cartSkid.GetComponent<Renderer>().sharedMaterial = matBtnHousing;
            Object.DestroyImmediate(cartSkid.GetComponent<Collider>());

            // Front Bumper / Pointer
            var cartBumper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cartBumper.name = "Cart_Bumper";
            cartBumper.transform.SetParent(cartGo.transform, false);
            cartBumper.transform.localPosition = new Vector3(0.095f, 0f, 0f);
            cartBumper.transform.localScale = new Vector3(0.015f, 0.04f, 0.14f);
            if (matBtnRed != null) cartBumper.GetComponent<Renderer>().sharedMaterial = matBtnRed;
            Object.DestroyImmediate(cartBumper.GetComponent<Collider>());

            // Pointer arrow indicator pointing at the track ruler
            var cartPointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cartPointer.name = "Cart_RulerPointer";
            cartPointer.transform.SetParent(cartGo.transform, false);
            cartPointer.transform.localPosition = new Vector3(0.095f, -0.01f, -0.09f);
            cartPointer.transform.localScale = new Vector3(0.005f, 0.02f, 0.025f);
            if (matBtnRed != null) cartPointer.GetComponent<Renderer>().sharedMaterial = matBtnRed;
            Object.DestroyImmediate(cartPointer.GetComponent<Collider>());

            // Cart Label Canvas
            var cartLabelGo = new GameObject("Cart_Label");
            cartLabelGo.transform.SetParent(cartGo.transform, false);
            cartLabelGo.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            cartLabelGo.transform.localRotation = Quaternion.Euler(90, 0, 0);
            cartLabelGo.transform.localScale = Vector3.one * 0.003f;
            var cartLabel = cartLabelGo.AddComponent<TextMeshProUGUI>();
            cartLabel.text = "<b>SÜRTÜNME KIZAĞI</b>\nm = 1.0 kg";
            cartLabel.fontSize = 11;
            cartLabel.alignment = TextAlignmentOptions.Center;
            cartLabel.rectTransform.sizeDelta = new Vector2(50, 30);

            var cartRb = cartGo.AddComponent<Rigidbody>();
            cartRb.mass = 1.0f;
            cartRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            cartRb.interpolation = RigidbodyInterpolation.Interpolate;

            var frictionCart = cartGo.AddComponent<FrictionCart>();
            var cartSo = new SerializedObject(frictionCart);
            cartSo.FindProperty("mass").floatValue = 1.0f;
            cartSo.FindProperty("surfaceSlot").objectReferenceValue = slot;
            cartSo.FindProperty("constrainToTrack").boolValue = true;
            cartSo.FindProperty("gravity").floatValue = 9.81f;
            cartSo.ApplyModifiedProperties();

            cartGo.AddComponent<ResetableObject>();

            // 9. Impulse Launcher
            var launcherGo = new GameObject("FrictionLauncher");
            launcherGo.transform.SetParent(trackAssembly.transform, false);
            launcherGo.transform.localPosition = new Vector3(-0.85f, 0.95f, 1.10f);

            var launcherHousing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            launcherHousing.name = "Launcher_Housing";
            launcherHousing.transform.SetParent(launcherGo.transform, false);
            launcherHousing.transform.localPosition = Vector3.zero;
            launcherHousing.transform.localScale = new Vector3(0.12f, 0.08f, 0.16f);
            if (matTable != null) launcherHousing.GetComponent<Renderer>().sharedMaterial = matTable;

            var piston = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            piston.name = "Launcher_Piston";
            piston.transform.SetParent(launcherGo.transform, false);
            piston.transform.localPosition = new Vector3(0.08f, 0f, 0f);
            piston.transform.localRotation = Quaternion.Euler(0, 0, 90);
            piston.transform.localScale = new Vector3(0.03f, 0.04f, 0.03f);
            if (matTrack != null) piston.GetComponent<Renderer>().sharedMaterial = matTrack;
            Object.DestroyImmediate(piston.GetComponent<Collider>());

            var launcher = launcherGo.AddComponent<FrictionImpulseLauncher>();
            var launcherSo = new SerializedObject(launcher);
            launcherSo.FindProperty("targetCart").objectReferenceValue = frictionCart;
            launcherSo.FindProperty("launchVelocity").floatValue = 1.2f;
            launcherSo.FindProperty("pistonArm").objectReferenceValue = piston.transform;
            launcherSo.FindProperty("pushedOffset").vector3Value = new Vector3(0.04f, 0f, 0f);
            launcherSo.ApplyModifiedProperties();

            // 10. Buttons (Clone Station 1's Launch & Reset buttons for guaranteed BNG compatibility)
            var st1LaunchBtn = GameObject.Find("LaunchButton");
            var st1ResetBtn = GameObject.Find("ResetButton");

            GameObject launchBtnGo;
            if (st1LaunchBtn != null)
            {
                launchBtnGo = Object.Instantiate(st1LaunchBtn, root.transform);
                launchBtnGo.name = "LaunchButton_Friction";
            }
            else
            {
                launchBtnGo = new GameObject("LaunchButton_Friction");
                launchBtnGo.transform.SetParent(root.transform, false);
            }
            launchBtnGo.transform.localPosition = new Vector3(-0.65f, 0.91f, 0.78f);
            launchBtnGo.transform.localRotation = Quaternion.identity;

            // Update Launch Button text and event
            var launchBtnScript = launchBtnGo.GetComponent<VRPhysicalButton>();
            if (launchBtnScript != null)
            {
                // Clear persistent calls and hook to launcher.Launch
                while (launchBtnScript.onPressed.GetPersistentEventCount() > 0)
                {
                    UnityEventTools.RemovePersistentListener(launchBtnScript.onPressed, 0);
                }
                UnityEventTools.AddPersistentListener(launchBtnScript.onPressed, launcher.Launch);

                var launchLabel = launchBtnGo.GetComponentInChildren<TextMeshProUGUI>(true);
                if (launchLabel != null) launchLabel.text = "<size=14><b>FIRLAT</b></size>\n<size=10>LAUNCH</size>";
            }

            GameObject resetBtnGo;
            if (st1ResetBtn != null)
            {
                resetBtnGo = Object.Instantiate(st1ResetBtn, root.transform);
                resetBtnGo.name = "ResetButton_Friction";
            }
            else
            {
                resetBtnGo = new GameObject("ResetButton_Friction");
                resetBtnGo.transform.SetParent(root.transform, false);
            }
            resetBtnGo.transform.localPosition = new Vector3(-0.45f, 0.91f, 0.78f);
            resetBtnGo.transform.localRotation = Quaternion.identity;

            var resetBtnScript = resetBtnGo.GetComponent<VRPhysicalButton>();
            var resetMgr = Object.FindFirstObjectByType<ResetManager>();
            if (resetBtnScript != null && resetMgr != null)
            {
                while (resetBtnScript.onPressed.GetPersistentEventCount() > 0)
                {
                    UnityEventTools.RemovePersistentListener(resetBtnScript.onPressed, 0);
                }
                UnityEventTools.AddPersistentListener(resetBtnScript.onPressed, resetMgr.ResetAll);

                var resetLabel = resetBtnGo.GetComponentInChildren<TextMeshProUGUI>(true);
                if (resetLabel != null) resetLabel.text = "<size=14><b>SIFIRLA</b></size>\n<size=10>RESET</size>";
            }

            // 11. Measurement System
            var measSysGo = new GameObject("FrictionMeasurementSystem");
            measSysGo.transform.SetParent(root.transform, false);
            measSysGo.transform.localPosition = Vector3.zero;
            var measSys = measSysGo.AddComponent<FrictionMeasurementSystem>();
            var measSo = new SerializedObject(measSys);
            measSo.FindProperty("cart").objectReferenceValue = frictionCart;
            measSo.FindProperty("surfaceSlot").objectReferenceValue = slot;
            measSo.FindProperty("launcher").objectReferenceValue = launcher;
            measSo.ApplyModifiedProperties();

            // 12. Measurement Display (Clone Station 1's display for pixel-perfect UI style)
            var st1Display = GameObject.Find("MeasurementDisplay");
            GameObject displayGo;
            if (st1Display != null)
            {
                displayGo = Object.Instantiate(st1Display, root.transform);
                displayGo.name = "MeasurementDisplay_Friction";
            }
            else
            {
                displayGo = new GameObject("MeasurementDisplay_Friction");
                displayGo.transform.SetParent(root.transform, false);
            }
            displayGo.transform.localPosition = new Vector3(0f, 1.20f, 1.40f);
            displayGo.transform.localRotation = Quaternion.identity;

            // Remove Station 1's DigitalDisplayUI
            var oldDisplayUi = displayGo.GetComponent<DigitalDisplayUI>();
            if (oldDisplayUi != null) Object.DestroyImmediate(oldDisplayUi);

            var frictionUi = displayGo.AddComponent<FrictionDisplayUI>();

            // Configure Texts on Display
            var texts = displayGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            var textMap = new Dictionary<string, TextMeshProUGUI>();
            foreach (var t in texts)
            {
                if (!textMap.ContainsKey(t.gameObject.name)) textMap[t.gameObject.name] = t;
            }

            if (textMap.ContainsKey("Title_Text"))
            {
                textMap["Title_Text"].text = "<color=#38BDF8><b>VR FİZİK LAB</b></color>  <color=#475569>|</color>  <b>İSTASYON 2: SÜRTÜNME KUVVETİ</b>";
            }
            if (textMap.ContainsKey("Status_Text"))
            {
                textMap["Status_Text"].text = "● HAZIR";
            }

            // Retitle Cards
            var headers = displayGo.transform.Find("ScreenCanvas/Panel_Background")?.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    if (h.gameObject.name == "Card_Header")
                    {
                        if (h.transform.parent.name == "Card_Telemetry")
                            h.text = "<b>CANLI ÖLÇÜMLER (TELEMETRİ)</b>";
                        else if (h.transform.parent.name == "Card_Theory")
                            h.text = "<b>SÜRTÜNME ANALİZİ & TEORİ</b>";
                    }
                }
            }

            // Retitle Left Card (Telemetry)
            if (textMap.ContainsKey("Label_Time")) textMap["Label_Time"].text = "Durma Süresi (t):";
            if (textMap.ContainsKey("Label_Dist")) textMap["Label_Dist"].text = "Durma Mesafesi (x):";
            if (textMap.ContainsKey("Label_Speed")) textMap["Label_Speed"].text = "Fırlatma Hızı (v0):";
            if (textMap.ContainsKey("Label_Accel")) textMap["Label_Accel"].text = "Yavaşlama İvmesi (a):";

            // Retitle Right Card (Theory / Friction)
            if (textMap.ContainsKey("Label_Force")) textMap["Label_Force"].text = "Aktif Yüzey:";
            if (textMap.ContainsKey("Value_Force")) textMap["Value_Force"].text = "Buz / Teflon";
            if (textMap.ContainsKey("Label_Mass")) textMap["Label_Mass"].text = "Deneysel µ (Hesaplanan):";
            if (textMap.ContainsKey("Value_Mass")) textMap["Value_Mass"].text = "--";
            if (textMap.ContainsKey("Label_Theo")) textMap["Label_Theo"].text = "Teorik µ (Referans):";
            if (textMap.ContainsKey("Value_Theo")) textMap["Value_Theo"].text = "0.050";
            if (textMap.ContainsKey("Label_Error")) textMap["Label_Error"].text = "Bağıl Hata (Sapma):";
            if (textMap.ContainsKey("Value_Error")) textMap["Value_Error"].text = "-- %";

            if (textMap.ContainsKey("Hint_Text"))
            {
                textMap["Hint_Text"].text = "Fırlatmak için masadaki <color=#FF5555><b>KIRMIZI</b></color> butona basın. Yeni yüzey için plakayı değiştirin.";
            }

            // Find status badge background image
            var statusBadgeImg = displayGo.transform.Find("ScreenCanvas/Panel_Background/Header_Bar/Status_Badge")?.GetComponent<UnityEngine.UI.Image>();

            // Wire FrictionDisplayUI serialized fields
            var uiSo = new SerializedObject(frictionUi);
            uiSo.FindProperty("measurementSystem").objectReferenceValue = measSys;
            uiSo.FindProperty("surfaceSlot").objectReferenceValue = slot;
            uiSo.FindProperty("cart").objectReferenceValue = frictionCart;

            if (textMap.ContainsKey("Status_Text")) uiSo.FindProperty("statusBadgeText").objectReferenceValue = textMap["Status_Text"];
            if (statusBadgeImg != null) uiSo.FindProperty("statusBadgeBg").objectReferenceValue = statusBadgeImg;
            if (textMap.ContainsKey("Value_Force")) uiSo.FindProperty("surfaceBadgeText").objectReferenceValue = textMap["Value_Force"];

            if (textMap.ContainsKey("Value_Time")) uiSo.FindProperty("timeText").objectReferenceValue = textMap["Value_Time"];
            if (textMap.ContainsKey("Value_Dist")) uiSo.FindProperty("distanceText").objectReferenceValue = textMap["Value_Dist"];
            if (textMap.ContainsKey("Value_Speed")) uiSo.FindProperty("speedText").objectReferenceValue = textMap["Value_Speed"];
            if (textMap.ContainsKey("Value_Accel")) uiSo.FindProperty("decelText").objectReferenceValue = textMap["Value_Accel"];

            if (textMap.ContainsKey("Value_Mass")) uiSo.FindProperty("expMuText").objectReferenceValue = textMap["Value_Mass"];
            if (textMap.ContainsKey("Value_Theo")) uiSo.FindProperty("theoMuText").objectReferenceValue = textMap["Value_Theo"];
            if (textMap.ContainsKey("Value_Error")) uiSo.FindProperty("errorText").objectReferenceValue = textMap["Value_Error"];

            if (textMap.ContainsKey("Hint_Text")) uiSo.FindProperty("hintText").objectReferenceValue = textMap["Hint_Text"];
            uiSo.ApplyModifiedProperties();

            // 13. Pre-install Ice Plate into track slot so station is immediately ready to test
            slot.InstallPlate(plateIce);

            // 14. Refresh ResetManager
            if (resetMgr != null)
            {
                resetMgr.RefreshResetablesList();
                EditorUtility.SetDirty(resetMgr);
            }

            // Mark Scene Dirty and Save
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log("[Station2Builder] Station 2 (Friction Experiment) successfully built and saved to scene!");
        }
    }
}
#endif
