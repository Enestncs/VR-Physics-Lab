using UnityEngine;
using TMPro;

namespace VRPhysicsLab.Visuals
{
    /// <summary>
    /// Procedural, low-poly 3D vector arrow with emissive color coding and a billboarded
    /// WorldSpace label. Designed for Quest 3 performance and clear physics visualization.
    /// </summary>
    public class VectorArrow3D : MonoBehaviour
    {
        private Transform shaftTransform;
        private Transform headTransform;
        private Transform labelRoot;
        private TMP_Text labelText;
        private Material arrowMaterial;
        private Camera cachedCamera;

        private const float ShaftRadius = 0.0085f;
        private const float HeadRadius = 0.020f;
        private const float HeadLength = 0.048f;
        private const float MinVisibleLength = 0.04f;

        private static Mesh sharedShaftMesh;
        private static Mesh sharedConeMesh;

        public void Initialize(string arrowName, Color color)
        {
            gameObject.name = arrowName;
            EnsureSharedMeshes();

            arrowMaterial = CreateArrowMaterial(color);

            // Shaft (unit length along +Z from 0 to 1)
            var shaftGo = new GameObject("Shaft");
            shaftGo.transform.SetParent(transform, false);
            shaftGo.AddComponent<MeshFilter>().sharedMesh = sharedShaftMesh;
            var shaftRenderer = shaftGo.AddComponent<MeshRenderer>();
            shaftRenderer.sharedMaterial = arrowMaterial;
            shaftRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shaftRenderer.receiveShadows = false;
            shaftTransform = shaftGo.transform;

            // Cone Head (length = 1 along +Z from 0 to 1)
            var headGo = new GameObject("Head");
            headGo.transform.SetParent(transform, false);
            headGo.AddComponent<MeshFilter>().sharedMesh = sharedConeMesh;
            var headRenderer = headGo.AddComponent<MeshRenderer>();
            headRenderer.sharedMaterial = arrowMaterial;
            headRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            headRenderer.receiveShadows = false;
            headTransform = headGo.transform;

            // WorldSpace Billboard Label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);

            var canvas = labelGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = labelGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220f, 44f);
            labelRoot = rt;
            labelRoot.localScale = Vector3.one * 0.00115f;

            var bgGo = new GameObject("Bg");
            bgGo.transform.SetParent(labelGo.transform, false);
            var bgImg = bgGo.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.05f, 0.08f, 0.14f, 0.86f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(labelGo.transform, false);
            labelText = txtGo.AddComponent<TextMeshProUGUI>();
            labelText.fontSize = 22f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.Lerp(color, Color.white, 0.35f);
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(4f, 2f);
            txtRt.offsetMax = new Vector2(-4f, -2f);
        }

        /// <summary>
        /// Updates the arrow's world-space origin, direction, total length (meters), and label text.
        /// </summary>
        public void SetVector(Vector3 worldOrigin, Vector3 worldDirection, float totalLength, string label, bool visible, float labelYOffset = 0.042f)
        {
            if (!visible || totalLength < MinVisibleLength || worldDirection.sqrMagnitude < 0.0001f)
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);

            Vector3 dir = worldDirection.normalized;
            transform.position = worldOrigin;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            float headLen = Mathf.Min(HeadLength, totalLength * 0.45f);
            float shaftLen = Mathf.Max(0.005f, totalLength - headLen);

            if (shaftTransform != null)
            {
                shaftTransform.localPosition = Vector3.zero;
                shaftTransform.localRotation = Quaternion.identity;
                shaftTransform.localScale = new Vector3(ShaftRadius, ShaftRadius, shaftLen);
            }

            if (headTransform != null)
            {
                headTransform.localPosition = new Vector3(0f, 0f, shaftLen);
                headTransform.localRotation = Quaternion.identity;
                headTransform.localScale = new Vector3(HeadRadius, HeadRadius, headLen);
            }

            if (labelRoot != null)
            {
                // Place label slightly above the arrow (or slightly to the left for near-vertical arrows like Normal force)
                bool isMostlyVertical = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.75f;
                Vector3 anchorWorld = isMostlyVertical
                    ? worldOrigin + dir * (totalLength * 0.72f) + new Vector3(-0.085f, 0.015f, -0.02f)
                    : worldOrigin + dir * (totalLength * 0.55f) + new Vector3(0f, labelYOffset + 0.012f, -0.02f);
                labelRoot.position = anchorWorld;

                if (cachedCamera == null) cachedCamera = Camera.main;
                if (cachedCamera != null && Vector3.Distance(cachedCamera.transform.position, anchorWorld) < 2.6f)
                {
                    Vector3 flatToCam = anchorWorld - cachedCamera.transform.position;
                    flatToCam.y = 0f;
                    if (flatToCam.sqrMagnitude > 0.0001f)
                    {
                        labelRoot.rotation = Quaternion.LookRotation(flatToCam.normalized, Vector3.up);
                    }
                }
                else
                {
                    // Face directly toward the front of the lab table (-Z)
                    labelRoot.rotation = Quaternion.identity;
                }
            }

            if (labelText != null && labelText.text != label)
            {
                labelText.text = label;
            }
        }

        private void OnDestroy()
        {
            if (arrowMaterial != null)
            {
                Destroy(arrowMaterial);
            }
        }

        private static Material CreateArrowMaterial(Color color)
        {
            bool isSrp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            Shader shader = isSrp
                ? (Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"))
                : (Shader.Find("Standard") ?? Shader.Find("Unlit/Color"));

            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.55f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.55f);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.45f);
            }
            return mat;
        }

        private static void EnsureSharedMeshes()
        {
            if (sharedShaftMesh == null)
            {
                sharedShaftMesh = BuildCylinderMesh(12);
                sharedShaftMesh.name = "ProceduralArrowShaft";
            }
            if (sharedConeMesh == null)
            {
                sharedConeMesh = BuildConeMesh(12);
                sharedConeMesh.name = "ProceduralArrowCone";
            }
        }

        // Unit cylinder along +Z (z from 0 to 1, radius = 1)
        private static Mesh BuildCylinderMesh(int segments)
        {
            var mesh = new Mesh();
            int vertCount = (segments + 1) * 2 + 2;
            var verts = new Vector3[vertCount];
            var norms = new Vector3[vertCount];
            var tris = new int[segments * 6 + segments * 3];

            for (int i = 0; i <= segments; i++)
            {
                float a = (i / (float)segments) * Mathf.PI * 2f;
                float c = Mathf.Cos(a);
                float s = Mathf.Sin(a);
                int idx = i * 2;
                verts[idx] = new Vector3(c, s, 0f);
                norms[idx] = new Vector3(c, s, 0f);
                verts[idx + 1] = new Vector3(c, s, 1f);
                norms[idx + 1] = new Vector3(c, s, 0f);
            }

            int t = 0;
            for (int i = 0; i < segments; i++)
            {
                int i0 = i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;
                int i3 = i0 + 3;
                tris[t++] = i0; tris[t++] = i2; tris[t++] = i1;
                tris[t++] = i2; tris[t++] = i3; tris[t++] = i1;
            }

            // Back cap at z = 0
            int backCenter = (segments + 1) * 2;
            verts[backCenter] = Vector3.zero;
            norms[backCenter] = Vector3.back;
            for (int i = 0; i < segments; i++)
            {
                tris[t++] = backCenter;
                tris[t++] = (i + 1) * 2;
                tris[t++] = i * 2;
            }

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        // Unit cone along +Z (base at z = 0 with radius = 1, tip at z = 1)
        private static Mesh BuildConeMesh(int segments)
        {
            var mesh = new Mesh();
            int baseStart = 0;
            int tipIndex = segments + 1;
            int capCenter = tipIndex + 1;
            int capRingStart = capCenter + 1;
            int vertCount = capRingStart + segments + 1;

            var verts = new Vector3[vertCount];
            var norms = new Vector3[vertCount];
            var tris = new int[segments * 6];

            verts[tipIndex] = new Vector3(0f, 0f, 1f);
            norms[tipIndex] = Vector3.forward;

            verts[capCenter] = Vector3.zero;
            norms[capCenter] = Vector3.back;

            for (int i = 0; i <= segments; i++)
            {
                float a = (i / (float)segments) * Mathf.PI * 2f;
                float c = Mathf.Cos(a);
                float s = Mathf.Sin(a);
                verts[baseStart + i] = new Vector3(c, s, 0f);
                norms[baseStart + i] = new Vector3(c, s, 0.5f).normalized;

                verts[capRingStart + i] = new Vector3(c, s, 0f);
                norms[capRingStart + i] = Vector3.back;
            }

            int t = 0;
            for (int i = 0; i < segments; i++)
            {
                // Side face
                tris[t++] = baseStart + i;
                tris[t++] = baseStart + i + 1;
                tris[t++] = tipIndex;
                // Base cap
                tris[t++] = capCenter;
                tris[t++] = capRingStart + i + 1;
                tris[t++] = capRingStart + i;
            }

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
