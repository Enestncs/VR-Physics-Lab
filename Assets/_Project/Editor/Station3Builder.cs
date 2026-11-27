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
using VRPhysicsLab.Visuals;

namespace VRPhysicsLab.Editor
{
    public static class Station3Builder
    {
        [MenuItem("VR Physics Lab/Build Station 3 (Inclined Plane Experiment)", priority = 101)]
        public static void BuildStation3()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            Undo.SetCurrentGroupName("Build Station 3");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Remove existing Station 3 root for clean idempotent rebuild
            const string station3RootName = "--- [EXPERIMENT 3 - INCLINED PLANE] ---";
            var existingRoot = GameObject.Find(station3RootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
                Debug.Log($"[Station3Builder] Removed old {station3RootName} for clean rebuild.");
            }

            // 2. Load Materials
            var matTable = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Table.mat");
            var matTrack = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Track.mat");
            var matCart = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Cart.mat");
            var matWeight = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_Weight.mat");
            var matBtnHousing = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_ButtonHousing.mat");
            var matBtnRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_ButtonRed.mat");
            var matBtnYellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_ButtonYellow.mat");
            var matSensorGreen = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_SensorGreen.mat");

            // Root GameObject for Station 3 (symmetrical to Station 2 on the left of Station 1)
            var root = new GameObject(station3RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Station 3 Root");
            root.transform.position = new Vector3(-2.4f, 0f, 0f);
            root.transform.rotation = Quaternion.identity;

            // 3. Station 3 Table (identical dimensions to Station 1 & 2)
            var table = new GameObject("LabTable_Station3");
            table.transform.SetParent(root.transform, false);
            table.transform.localPosition = Vector3.zero;

            var tabletop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tabletop.name = "TableTop";
            tabletop.transform.SetParent(table.transform, false);
            tabletop.transform.localPosition = new Vector3(0f, 0.88f, 1.10f);
            tabletop.transform.localScale = new Vector3(2.2f, 0.05f, 0.90f);
            if (matTable != null) tabletop.GetComponent<Renderer>().sharedMaterial = matTable;

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

            // 4. Station Signboard
            var signPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signPost.name = "StationSign_Post";
            signPost.transform.SetParent(table.transform, false);
            signPost.transform.localPosition = new Vector3(-1.02f, 1.15f, 0.70f);
            signPost.transform.localScale = new Vector3(0.04f, 0.50f, 0.04f);
            if (matBtnHousing != null) signPost.GetComponent<Renderer>().sharedMaterial = matBtnHousing;

            var signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = "StationSign_Board";
            signBoard.transform.SetParent(table.transform, false);
            signBoard.transform.localPosition = new Vector3(-1.02f, 1.43f, 0.70f);
            signBoard.transform.localScale = new Vector3(0.45f, 0.22f, 0.04f);
            if (matTable != null) signBoard.GetComponent<Renderer>().sharedMaterial = matTable;

            var signCanvasGo = new GameObject("SignCanvas");
            signCanvasGo.transform.SetParent(table.transform, false);
            signCanvasGo.transform.localPosition = new Vector3(-1.02f, 1.43f, 0.678f);
            signCanvasGo.transform.localRotation = Quaternion.identity;
            signCanvasGo.transform.localScale = Vector3.one * 0.0018f;
            var signCanvas = signCanvasGo.AddComponent<Canvas>();
            signCanvas.renderMode = RenderMode.WorldSpace;
            signCanvasGo.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 110);
            var signTextGo = new GameObject("Sign_Text");
            signTextGo.transform.SetParent(signCanvasGo.transform, false);
            var signText = signTextGo.AddComponent<TextMeshProUGUI>();
            signText.text = "<size=24><b><color=#38BDF8>İSTASYON 3</color></b></size>\n<size=18>Eğik Düzlem Deneyi</size>\n<size=12><color=#94A3B8>a = g · sin(θ)</color></size>";
            signText.alignment = TextAlignmentOptions.Center;
            signText.rectTransform.sizeDelta = new Vector2(240, 110);

            // 5. Adjustable Ramp Assembly
            var rampRoot = new GameObject("InclinedRamp_Assembly");
            rampRoot.transform.SetParent(root.transform, false);
            rampRoot.transform.localPosition = Vector3.zero;

            // Hinge Base Block at bottom-right end of ramp
            var hingeBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hingeBase.name = "Ramp_HingeBase";
            hingeBase.transform.SetParent(rampRoot.transform, false);
            hingeBase.transform.localPosition = new Vector3(0.78f, 0.915f, 1.10f);
            hingeBase.transform.localScale = new Vector3(0.14f, 0.025f, 0.28f);
            if (matBtnHousing != null) hingeBase.GetComponent<Renderer>().sharedMaterial = matBtnHousing;

            var hingeAxle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hingeAxle.name = "Ramp_HingeAxle";
            hingeAxle.transform.SetParent(rampRoot.transform, false);
            hingeAxle.transform.localPosition = new Vector3(0.78f, 0.930f, 1.10f);
            hingeAxle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hingeAxle.transform.localScale = new Vector3(0.028f, 0.145f, 0.028f);
            if (matWeight != null) hingeAxle.GetComponent<Renderer>().sharedMaterial = matWeight;

            // Telescoping Support Column under elevated end
            var supportCol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            supportCol.name = "Ramp_SupportColumn";
            supportCol.transform.SetParent(rampRoot.transform, false);
            supportCol.transform.localPosition = new Vector3(-0.40f, 1.10f, 1.10f);
            supportCol.transform.localScale = new Vector3(0.045f, 0.20f, 0.045f);
            if (matBtnHousing != null) supportCol.GetComponent<Renderer>().sharedMaterial = matBtnHousing;

            // Ramp Pivot (origin at hinge axle; ramp extends along -X from 0 to -1.50m)
            var rampPivot = new GameObject("Ramp_Pivot");
            rampPivot.transform.SetParent(rampRoot.transform, false);
            rampPivot.transform.localPosition = new Vector3(0.78f, 0.930f, 1.10f);
            rampPivot.transform.localRotation = Quaternion.identity;

            // Ramp Deck (length = 1.50m, width = 0.24m)
            var rampDeck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampDeck.name = "Ramp_Deck";
            rampDeck.transform.SetParent(rampPivot.transform, false);
            rampDeck.transform.localPosition = new Vector3(-0.75f, 0f, 0f);
            rampDeck.transform.localScale = new Vector3(1.50f, 0.03f, 0.24f);
            if (matTrack != null) rampDeck.GetComponent<Renderer>().sharedMaterial = matTrack;

            // Side Guide Rails
            var railFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            railFront.name = "Ramp_RailFront";
            railFront.transform.SetParent(rampPivot.transform, false);
            railFront.transform.localPosition = new Vector3(-0.75f, 0.025f, -0.12f);
            railFront.transform.localScale = new Vector3(1.50f, 0.035f, 0.02f);
            if (matTrack != null) railFront.GetComponent<Renderer>().sharedMaterial = matTrack;

            var railBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            railBack.name = "Ramp_RailBack";
            railBack.transform.SetParent(rampPivot.transform, false);
            railBack.transform.localPosition = new Vector3(-0.75f, 0.025f, 0.12f);
            railBack.transform.localScale = new Vector3(1.50f, 0.035f, 0.02f);
            if (matTrack != null) railBack.GetComponent<Renderer>().sharedMaterial = matTrack;

            // Top & Bottom Bumpers
            var topBumper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topBumper.name = "Ramp_TopStop";
            topBumper.transform.SetParent(rampPivot.transform, false);
            topBumper.transform.localPosition = new Vector3(-1.48f, 0.035f, 0f);
            topBumper.transform.localScale = new Vector3(0.03f, 0.055f, 0.22f);
            if (matBtnHousing != null) topBumper.GetComponent<Renderer>().sharedMaterial = matBtnHousing;

            var bottomBumper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottomBumper.name = "Ramp_BottomBumper";
            bottomBumper.transform.SetParent(rampPivot.transform, false);
            bottomBumper.transform.localPosition = new Vector3(0.01f, 0.035f, 0f);
            bottomBumper.transform.localScale = new Vector3(0.03f, 0.055f, 0.22f);
            if (matBtnRed != null) bottomBumper.GetComponent<Renderer>().sharedMaterial = matBtnRed;

            // Start & Finish Line Strips on Ramp Deck (distance = 1.20m: from -1.32 to -0.12)
            var startLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            startLine.name = "Ramp_StartLine";
            startLine.transform.SetParent(rampPivot.transform, false);
            startLine.transform.localPosition = new Vector3(-1.32f, 0.016f, 0f);
            startLine.transform.localScale = new Vector3(0.015f, 0.004f, 0.21f);
            if (matSensorGreen != null) startLine.GetComponent<Renderer>().sharedMaterial = matSensorGreen;
            Object.DestroyImmediate(startLine.GetComponent<Collider>());

            var finishLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            finishLine.name = "Ramp_FinishLine";
            finishLine.transform.SetParent(rampPivot.transform, false);
            finishLine.transform.localPosition = new Vector3(-0.12f, 0.016f, 0f);
            finishLine.transform.localScale = new Vector3(0.015f, 0.004f, 0.21f);
            if (matBtnYellow != null) finishLine.GetComponent<Renderer>().sharedMaterial = matBtnYellow;
            Object.DestroyImmediate(finishLine.GetComponent<Collider>());

            // Angle Readout Badge right near the hinge (offset clear of finished cart)
            var angleCanvasGo = new GameObject("AngleReadout_Canvas");
            angleCanvasGo.transform.SetParent(rampRoot.transform, false);
            angleCanvasGo.transform.localPosition = new Vector3(0.84f, 0.965f, 0.90f);
            angleCanvasGo.transform.localRotation = Quaternion.identity;
            angleCanvasGo.transform.localScale = Vector3.one * 0.0014f;
            var angleCanvas = angleCanvasGo.AddComponent<Canvas>();
            angleCanvas.renderMode = RenderMode.WorldSpace;
            angleCanvasGo.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 44);

            var angleBgGo = new GameObject("Bg");
            angleBgGo.transform.SetParent(angleCanvasGo.transform, false);
            var angleBgImg = angleBgGo.AddComponent<UnityEngine.UI.Image>();
            angleBgImg.color = new Color(0.07f, 0.11f, 0.18f, 0.94f);
            var angleBgRt = angleBgGo.GetComponent<RectTransform>();
            angleBgRt.anchorMin = Vector2.zero;
            angleBgRt.anchorMax = Vector2.one;
            angleBgRt.offsetMin = Vector2.zero;
            angleBgRt.offsetMax = Vector2.zero;

            var angleTxtGo = new GameObject("Text");
            angleTxtGo.transform.SetParent(angleCanvasGo.transform, false);
            var angleTmp = angleTxtGo.AddComponent<TextMeshProUGUI>();
            angleTmp.text = "<b>θ = 20°</b>";
            angleTmp.fontSize = 24;
            angleTmp.color = new Color(0.22f, 0.74f, 0.97f, 1f);
            angleTmp.alignment = TextAlignmentOptions.Center;
            var angleTxtRt = angleTxtGo.GetComponent<RectTransform>();
            angleTxtRt.anchorMin = Vector2.zero;
            angleTxtRt.anchorMax = Vector2.one;
            angleTxtRt.offsetMin = Vector2.zero;
            angleTxtRt.offsetMax = Vector2.zero;

            // Attach AdjustableRamp script
            var adjustableRamp = rampRoot.AddComponent<AdjustableRamp>();
            var rampSo = new SerializedObject(adjustableRamp);
            rampSo.FindProperty("defaultAngle").floatValue = 20f;
            rampSo.FindProperty("minAngle").floatValue = 10f;
            rampSo.FindProperty("maxAngle").floatValue = 45f;
            rampSo.FindProperty("angleStep").floatValue = 5f;
            rampSo.FindProperty("currentAngle").floatValue = 20f;
            rampSo.FindProperty("rampPivot").objectReferenceValue = rampPivot.transform;
            rampSo.FindProperty("supportColumn").objectReferenceValue = supportCol.transform;
            rampSo.FindProperty("supportAttachLocalX").floatValue = -1.25f;
            rampSo.FindProperty("tableSurfaceLocalY").floatValue = 0.905f;
            rampSo.FindProperty("angleReadoutText").objectReferenceValue = angleTmp;
            rampSo.ApplyModifiedProperties();

            // 6. Inclined Cart (uses car.fbx with our tuned wheel fit, riding on Ramp_Pivot)
            var cartGo = new GameObject("InclinedCart");
            cartGo.transform.SetParent(rampPivot.transform, false);
            cartGo.transform.localPosition = new Vector3(-1.32f, 0.052f, 0f);
            cartGo.transform.localRotation = Quaternion.identity;
            cartGo.transform.localScale = Vector3.one;

            var cartCol = cartGo.AddComponent<BoxCollider>();
            cartCol.center = new Vector3(0f, 0.02f, 0f);
            cartCol.size = new Vector3(0.24f, 0.09f, 0.16f);

            var carFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Models/car.fbx");
            if (carFbx != null)
            {
                var carModel = (GameObject)PrefabUtility.InstantiatePrefab(carFbx, cartGo.transform);
                PrefabUtility.UnpackPrefabInstance(carModel, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                carModel.name = "Car_Model";
                carModel.transform.localPosition = new Vector3(0.0008f, 0.0960f, 0f);
                carModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                carModel.transform.localScale = new Vector3(0.80f, 0.80f, 0.80f);
                foreach (var c in carModel.GetComponentsInChildren<Collider>(true))
                {
                    Object.DestroyImmediate(c);
                }
                var wheels = carModel.transform.Find("Cube.004");
                if (wheels != null)
                {
                    wheels.localPosition = new Vector3(0.0272f, -0.1025f, 0f);
                    wheels.localScale = new Vector3(12.5674f, 10.7000f, 4.8738f);
                }
            }
            else
            {
                var fallbackBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fallbackBody.name = "Cart_Body";
                fallbackBody.transform.SetParent(cartGo.transform, false);
                fallbackBody.transform.localScale = new Vector3(0.22f, 0.045f, 0.16f);
                if (matCart != null) fallbackBody.GetComponent<Renderer>().sharedMaterial = matCart;
                Object.DestroyImmediate(fallbackBody.GetComponent<Collider>());
            }

            var inclinedCart = cartGo.AddComponent<InclinedCart>();
            var cartSo = new SerializedObject(inclinedCart);
            cartSo.FindProperty("ramp").objectReferenceValue = adjustableRamp;
            cartSo.FindProperty("mass").floatValue = 1.0f;
            cartSo.FindProperty("gravity").floatValue = 9.81f;
            cartSo.FindProperty("rollingFriction").floatValue = 0.0f;
            cartSo.FindProperty("startLocalX").floatValue = -1.32f;
            cartSo.FindProperty("endLocalX").floatValue = -0.12f;
            cartSo.FindProperty("deckLocalY").floatValue = 0.052f;
            cartSo.ApplyModifiedProperties();

            var vectorVis = cartGo.AddComponent<ForceVectorVisualizer>();
            var visSo = new SerializedObject(vectorVis);
            visSo.FindProperty("stationType").enumValueIndex = (int)VectorStationType.Station3_InclinedPlane;
            visSo.ApplyModifiedProperties();

            // Apply initial 20° tilt immediately in Editor so scene looks ready right away
            adjustableRamp.ApplyImmediate();

            // 7. Control Buttons along the front edge of the table (Release, Angle -, Angle +, Reset)
            var st1LaunchBtn = GameObject.Find("--- [EXPERIMENT 1 - NEWTON] ---/Track_Assembly/ForceLauncher/LaunchButton") ?? GameObject.Find("LaunchButton");
            var st1ResetBtn = GameObject.Find("--- [EXPERIMENT 1 - NEWTON] ---/ResetButton") ?? GameObject.Find("ResetButton");
            var resetMgr = Object.FindFirstObjectByType<ResetManager>();

            System.Func<GameObject, string, Vector3, string, Material, VRPhysicalButton> spawnButton =
                (template, btnName, localPos, labelText, plungerMat) =>
            {
                GameObject btnGo = template != null
                    ? Object.Instantiate(template, root.transform)
                    : new GameObject(btnName);
                btnGo.name = btnName;
                btnGo.transform.SetParent(root.transform, false);
                btnGo.transform.localPosition = localPos;
                btnGo.transform.localRotation = Quaternion.identity;
                btnGo.transform.localScale = Vector3.one;

                if (plungerMat != null)
                {
                    var gfx = btnGo.transform.Find("Plunger/Plunger_GFX");
                    if (gfx != null)
                    {
                        var r = gfx.GetComponent<Renderer>();
                        if (r != null) r.sharedMaterial = plungerMat;
                    }
                }

                var tmp = btnGo.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null) tmp.text = labelText;

                var btnScript = btnGo.GetComponent<VRPhysicalButton>();
                if (btnScript != null)
                {
                    while (btnScript.onPressed.GetPersistentEventCount() > 0)
                    {
                        UnityEventTools.RemovePersistentListener(btnScript.onPressed, 0);
                    }
                }
                return btnScript;
            };

            // Reset (Yellow) at left-front
            var resetBtn = spawnButton(st1ResetBtn, "ResetButton_Inclined", new Vector3(-0.75f, 0.905f, 0.78f), "<b>SIFIRLA</b>\n<size=14>RESET</size>", matBtnYellow);
            if (resetBtn != null && resetMgr != null)
            {
                UnityEventTools.AddPersistentListener(resetBtn.onPressed, resetMgr.ResetAll);
            }

            // Angle Decrease (Blue)
            var angleDownBtn = spawnButton(st1ResetBtn, "AngleDownButton", new Vector3(-0.22f, 0.905f, 0.78f), "<b>AÇI -5°</b>\n<size=14>AZALT</size>", matCart);
            if (angleDownBtn != null)
            {
                var btnSo = new SerializedObject(angleDownBtn);
                btnSo.FindProperty("keyboardShortcut").intValue = (int)KeyCode.Minus;
                btnSo.FindProperty("pressedEmission").colorValue = new Color(0.15f, 0.65f, 0.95f, 1f);
                btnSo.ApplyModifiedProperties();
                UnityEventTools.AddPersistentListener(angleDownBtn.onPressed, adjustableRamp.DecreaseAngle);
            }

            // Angle Increase (Blue)
            var angleUpBtn = spawnButton(st1ResetBtn, "AngleUpButton", new Vector3(0.02f, 0.905f, 0.78f), "<b>AÇI +5°</b>\n<size=14>ARTIR</size>", matCart);
            if (angleUpBtn != null)
            {
                var btnSo = new SerializedObject(angleUpBtn);
                btnSo.FindProperty("keyboardShortcut").intValue = (int)KeyCode.Equals;
                btnSo.FindProperty("pressedEmission").colorValue = new Color(0.15f, 0.65f, 0.95f, 1f);
                btnSo.ApplyModifiedProperties();
                UnityEventTools.AddPersistentListener(angleUpBtn.onPressed, adjustableRamp.IncreaseAngle);
            }

            // Release Cart (Red)
            var releaseBtn = spawnButton(st1LaunchBtn, "ReleaseButton_Inclined", new Vector3(0.42f, 0.905f, 0.78f), "<b>BIRAK</b>\n<size=14>RELEASE</size>", matBtnRed);
            if (releaseBtn != null)
            {
                UnityEventTools.AddPersistentListener(releaseBtn.onPressed, inclinedCart.Release);
            }

            // 8. Measurement System
            var measSysGo = new GameObject("InclinedMeasurementSystem");
            measSysGo.transform.SetParent(root.transform, false);
            measSysGo.transform.localPosition = Vector3.zero;
            var measSys = measSysGo.AddComponent<InclinedMeasurementSystem>();
            var measSo = new SerializedObject(measSys);
            measSo.FindProperty("ramp").objectReferenceValue = adjustableRamp;
            measSo.FindProperty("cart").objectReferenceValue = inclinedCart;
            measSo.ApplyModifiedProperties();

            // 9. Measurement Display Monitor (cloned from Station 1 for consistent lab styling)
            var st1Display = GameObject.Find("MeasurementDisplay");
            GameObject displayGo;
            if (st1Display != null)
            {
                displayGo = Object.Instantiate(st1Display, root.transform);
                displayGo.name = "MeasurementDisplay_Inclined";
            }
            else
            {
                displayGo = new GameObject("MeasurementDisplay_Inclined");
                displayGo.transform.SetParent(root.transform, false);
            }
            // Positioned high & to the right so even at 45° the ramp never blocks the monitor
            displayGo.transform.localPosition = new Vector3(0.62f, 1.68f, 1.44f);
            displayGo.transform.localRotation = Quaternion.identity;

            // Extend Display_Stand cleanly down to the tabletop (world Y = 0.905 -> localY = -0.775)
            var stand = displayGo.transform.Find("Display_Stand");
            if (stand != null)
            {
                stand.localPosition = new Vector3(stand.localPosition.x, -0.4775f, stand.localPosition.z);
                stand.localScale = new Vector3(stand.localScale.x, 0.595f, stand.localScale.z);
            }

            var oldDisplayUi = displayGo.GetComponent<DigitalDisplayUI>();
            if (oldDisplayUi != null) Object.DestroyImmediate(oldDisplayUi);

            var inclinedUi = displayGo.AddComponent<InclinedDisplayUI>();

            var texts = displayGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            var textMap = new Dictionary<string, TextMeshProUGUI>();
            foreach (var t in texts)
            {
                if (!textMap.ContainsKey(t.gameObject.name)) textMap[t.gameObject.name] = t;
            }

            if (textMap.ContainsKey("Title_Text"))
                textMap["Title_Text"].text = "<color=#38BDF8><b>VR FİZİK LAB</b></color>  <color=#475569>|</color>  <b>İSTASYON 3: EĞİK DÜZLEM</b>";
            if (textMap.ContainsKey("Status_Text"))
                textMap["Status_Text"].text = "● HAZIR";

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
                            h.text = "<b>EĞİK DÜZLEM ANALİZİ & TEORİ</b>";
                    }
                }
            }

            if (textMap.ContainsKey("Label_Time")) textMap["Label_Time"].text = "İniş Süresi (t):";
            if (textMap.ContainsKey("Label_Dist")) textMap["Label_Dist"].text = "Eğim Mesafesi (d):";
            if (textMap.ContainsKey("Label_Speed")) textMap["Label_Speed"].text = "Anlık / Bitiş Hızı (v):";
            if (textMap.ContainsKey("Label_Accel")) textMap["Label_Accel"].text = "Deneysel İvme (a):";

            if (textMap.ContainsKey("Label_Force")) textMap["Label_Force"].text = "Rampa Açısı (θ):";
            if (textMap.ContainsKey("Value_Force")) textMap["Value_Force"].text = "<b>20°</b>  (sinθ=0.34)";
            if (textMap.ContainsKey("Label_Mass")) textMap["Label_Mass"].text = "Eğim Kuvveti (mg·sinθ):";
            if (textMap.ContainsKey("Value_Mass")) textMap["Value_Mass"].text = "3.36 N";
            if (textMap.ContainsKey("Label_Theo")) textMap["Label_Theo"].text = "Teorik İvme (g·sinθ):";
            if (textMap.ContainsKey("Value_Theo")) textMap["Value_Theo"].text = "3.36 m/s²";
            if (textMap.ContainsKey("Label_Error")) textMap["Label_Error"].text = "Bağıl Hata (Sapma):";
            if (textMap.ContainsKey("Value_Error")) textMap["Value_Error"].text = "-- %";

            if (textMap.ContainsKey("Hint_Text"))
                textMap["Hint_Text"].text = "Açıyı <color=#38BDF8><b>MAVİ (+/-)</b></color> butonlarla ayarlayın, bırakmak için <color=#FF5555><b>KIRMIZI</b></color> butona basın.";

            var statusBadgeImg = displayGo.transform.Find("ScreenCanvas/Panel_Background/Header_Bar/Status_Badge")?.GetComponent<UnityEngine.UI.Image>();

            var uiSo = new SerializedObject(inclinedUi);
            uiSo.FindProperty("measurementSystem").objectReferenceValue = measSys;
            uiSo.FindProperty("ramp").objectReferenceValue = adjustableRamp;
            uiSo.FindProperty("cart").objectReferenceValue = inclinedCart;
            if (textMap.ContainsKey("Status_Text")) uiSo.FindProperty("statusBadgeText").objectReferenceValue = textMap["Status_Text"];
            if (statusBadgeImg != null) uiSo.FindProperty("statusBadgeBg").objectReferenceValue = statusBadgeImg;

            if (textMap.ContainsKey("Value_Time")) uiSo.FindProperty("timeText").objectReferenceValue = textMap["Value_Time"];
            if (textMap.ContainsKey("Value_Dist")) uiSo.FindProperty("distanceText").objectReferenceValue = textMap["Value_Dist"];
            if (textMap.ContainsKey("Value_Speed")) uiSo.FindProperty("speedText").objectReferenceValue = textMap["Value_Speed"];
            if (textMap.ContainsKey("Value_Accel")) uiSo.FindProperty("accelText").objectReferenceValue = textMap["Value_Accel"];

            if (textMap.ContainsKey("Value_Force")) uiSo.FindProperty("angleText").objectReferenceValue = textMap["Value_Force"];
            if (textMap.ContainsKey("Value_Mass")) uiSo.FindProperty("parallelForceText").objectReferenceValue = textMap["Value_Mass"];
            if (textMap.ContainsKey("Value_Theo")) uiSo.FindProperty("theoAccelText").objectReferenceValue = textMap["Value_Theo"];
            if (textMap.ContainsKey("Value_Error")) uiSo.FindProperty("errorText").objectReferenceValue = textMap["Value_Error"];
            if (textMap.ContainsKey("Hint_Text")) uiSo.FindProperty("hintText").objectReferenceValue = textMap["Hint_Text"];
            uiSo.ApplyModifiedProperties();

            // 10. Refresh ResetManager
            if (resetMgr != null)
            {
                resetMgr.RefreshResetablesList();
                EditorUtility.SetDirty(resetMgr);
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log("[Station3Builder] Station 3 (Inclined Plane Experiment) successfully built and saved to scene!");
        }
    }
}
#endif
