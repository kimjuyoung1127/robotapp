// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.Kinematics;
using KineTutor3D.Math;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 2D FK 다이어그램을 RawImage + Texture2D에 렌더링합니다.
    /// 그리드/좌표축, 관절별 색상 링크, 각도 arc, EE 좌표를 실시간 표시합니다.
    /// </summary>
    public class FKDiagramPanel : MonoBehaviour, IVisibilityControllable
    {
        private RawImage rawImage;
        private Texture2D texture;
        private GameObject panel;
        private Text eeLabel;

        private int resolution;
        private float worldExtent = 4f;
        private bool initialized;

        private void OnEnable()
        {
            EnsurePresentation();
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// FK 결과를 바탕으로 다이어그램을 새로 그립니다.
        /// </summary>
        public void Refresh(DHLink[] links, double[] jointValues)
        {
            EnsurePresentation();
            if (!initialized || links == null || jointValues == null || links.Length == 0)
            {
                return;
            }

            var transforms = ForwardKinematics.ComputeAll(links, jointValues);
            ClearTexture();
            DrawGrid();
            DrawAxes();
            DrawLinks(transforms);
            DrawJoints(transforms);
            DrawAngleArcs(links, jointValues, transforms);
            DrawEndEffectorMarker(transforms);
            UpdateEELabel(transforms);
            texture.Apply();
        }

        private void EnsurePresentation()
        {
            if (initialized)
            {
                return;
            }

            resolution = UIDesignTokens.Size.DiagramResolution;
            texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            panel = gameObject;
            rawImage = GetComponentInChildren<RawImage>();
            if (rawImage == null)
            {
                var imgGo = new GameObject("DiagramImage", typeof(RectTransform), typeof(RawImage));
                imgGo.transform.SetParent(transform, false);
                var rt = imgGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rawImage = imgGo.GetComponent<RawImage>();
            }

            rawImage.texture = texture;

            eeLabel = GetComponentInChildren<Text>();
            if (eeLabel == null)
            {
                eeLabel = UIComponentFactory.CreateText(
                    transform,
                    "EELabel",
                    TypographyPreset.Caption,
                    UIDesignTokens.Colors.DiagramEE,
                    string.Empty);
                eeLabel.alignment = TextAnchor.LowerLeft;
                var labelRt = eeLabel.rectTransform;
                labelRt.anchorMin = new Vector2(0f, 0f);
                labelRt.anchorMax = new Vector2(1f, 0f);
                labelRt.pivot = new Vector2(0.5f, 0f);
                labelRt.offsetMin = new Vector2(UIDesignTokens.Space.Xs, UIDesignTokens.Space.Xs);
                labelRt.offsetMax = new Vector2(-UIDesignTokens.Space.Xs, UIDesignTokens.Space.Xs + UIDesignTokens.Type.Caption + UIDesignTokens.Space.Xxs);
            }

            initialized = true;
        }

        private void ClearTexture()
        {
            var bg = UIDesignTokens.Colors.SurfaceBase;
            var pixels = texture.GetPixels32();
            var c32 = new Color32((byte)(bg.r * 255), (byte)(bg.g * 255), (byte)(bg.b * 255), 255);
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = c32;
            }

            texture.SetPixels32(pixels);
        }

        private Vector2Int WorldToPixel(double wx, double wy)
        {
            var half = resolution / 2;
            var scale = half / worldExtent;
            var px = Mathf.Clamp((int)(half + wx * scale), 0, resolution - 1);
            var py = Mathf.Clamp((int)(half + wy * scale), 0, resolution - 1);
            return new Vector2Int(px, py);
        }

        private void DrawGrid()
        {
            var gridColor = UIDesignTokens.Colors.DiagramGrid;
            var step = 1.0;

            for (var w = -worldExtent; w <= worldExtent; w += (float)step)
            {
                var from = WorldToPixel(w, -worldExtent);
                var to = WorldToPixel(w, worldExtent);
                DrawLine(from.x, from.y, to.x, to.y, gridColor);

                from = WorldToPixel(-worldExtent, w);
                to = WorldToPixel(worldExtent, w);
                DrawLine(from.x, from.y, to.x, to.y, gridColor);
            }
        }

        private void DrawAxes()
        {
            var axisColor = UIDesignTokens.Colors.DiagramAxis;
            var origin = WorldToPixel(0, 0);
            var xEnd = WorldToPixel(worldExtent, 0);
            var yEnd = WorldToPixel(0, worldExtent);

            DrawLineThick(origin.x, origin.y, xEnd.x, xEnd.y, axisColor, 2);
            DrawLineThick(origin.x, origin.y, yEnd.x, yEnd.y, axisColor, 2);
        }

        private void DrawLinks(Mat4D[] transforms)
        {
            var prevX = 0.0;
            var prevY = 0.0;

            for (var i = 0; i < transforms.Length; i++)
            {
                var pos = transforms[i].ExtractPosition();
                var color = GetLinkColor(i);

                var from = WorldToPixel(prevX, prevY);
                var to = WorldToPixel(pos.X, pos.Y);
                DrawLineThick(from.x, from.y, to.x, to.y, color, 3);

                prevX = pos.X;
                prevY = pos.Y;
            }
        }

        private void DrawJoints(Mat4D[] transforms)
        {
            var jointColor = UIDesignTokens.Colors.DiagramJoint;
            var originPx = WorldToPixel(0, 0);
            DrawFilledCircle(originPx.x, originPx.y, 5, jointColor);

            for (var i = 0; i < transforms.Length; i++)
            {
                var pos = transforms[i].ExtractPosition();
                var px = WorldToPixel(pos.X, pos.Y);
                DrawFilledCircle(px.x, px.y, 4, jointColor);
            }
        }

        private void DrawAngleArcs(DHLink[] links, double[] jointValues, Mat4D[] transforms)
        {
            var originPx = WorldToPixel(0, 0);
            DrawArc(originPx.x, originPx.y, 20, 0, jointValues[0], GetLinkColor(0));

            for (var i = 1; i < transforms.Length; i++)
            {
                var parentPos = transforms[i - 1].ExtractPosition();
                var px = WorldToPixel(parentPos.X, parentPos.Y);

                var parentAngle = 0.0;
                for (var j = 0; j <= i - 1; j++)
                {
                    parentAngle += jointValues[j];
                }

                DrawArc(px.x, px.y, 16, parentAngle, parentAngle + jointValues[i], GetLinkColor(i));
            }
        }

        private void DrawEndEffectorMarker(Mat4D[] transforms)
        {
            var eePos = transforms[transforms.Length - 1].ExtractPosition();
            var eePx = WorldToPixel(eePos.X, eePos.Y);
            var eeColor = UIDesignTokens.Colors.DiagramEE;
            DrawFilledCircle(eePx.x, eePx.y, 6, eeColor);
            DrawCircleOutline(eePx.x, eePx.y, 8, eeColor);
        }

        private void UpdateEELabel(Mat4D[] transforms)
        {
            if (eeLabel == null)
            {
                return;
            }

            var eePos = transforms[transforms.Length - 1].ExtractPosition();
            eeLabel.text = $"EE: ({eePos.X:F2}, {eePos.Y:F2})";
        }

        private Color GetLinkColor(int index)
        {
            switch (index % 6)
            {
                case 0: return UIDesignTokens.Colors.DiagramLink1;
                case 1: return UIDesignTokens.Colors.DiagramLink2;
                case 2: return UIDesignTokens.Colors.DiagramLink3;
                case 3: return UIDesignTokens.Colors.DiagramLink4;
                case 4: return UIDesignTokens.Colors.DiagramLink5;
                case 5: return UIDesignTokens.Colors.DiagramLink6;
                default: return UIDesignTokens.Colors.DiagramLink1;
            }
        }

        private void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            var dx = Mathf.Abs(x1 - x0);
            var dy = Mathf.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                SetPixelSafe(x0, y0, color);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private void DrawLineThick(int x0, int y0, int x1, int y1, Color color, int thickness)
        {
            for (var t = -thickness / 2; t <= thickness / 2; t++)
            {
                var dx = Mathf.Abs(x1 - x0);
                var dy = Mathf.Abs(y1 - y0);

                if (dx >= dy)
                {
                    DrawLine(x0, y0 + t, x1, y1 + t, color);
                }
                else
                {
                    DrawLine(x0 + t, y0, x1 + t, y1, color);
                }
            }
        }

        private void DrawFilledCircle(int cx, int cy, int radius, Color color)
        {
            for (var y = -radius; y <= radius; y++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        SetPixelSafe(cx + x, cy + y, color);
                    }
                }
            }
        }

        private void DrawCircleOutline(int cx, int cy, int radius, Color color)
        {
            var steps = Mathf.Max(36, radius * 4);
            for (var i = 0; i < steps; i++)
            {
                var angle = 2f * Mathf.PI * i / steps;
                var px = cx + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                var py = cy + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                SetPixelSafe(px, py, color);
            }
        }

        private void DrawArc(int cx, int cy, int radius, double startRad, double endRad, Color color)
        {
            var steps = 36;
            var delta = endRad - startRad;
            for (var i = 0; i <= steps; i++)
            {
                var t = startRad + delta * i / steps;
                var px = cx + Mathf.RoundToInt(Mathf.Cos((float)t) * radius);
                var py = cy + Mathf.RoundToInt(Mathf.Sin((float)t) * radius);
                SetPixelSafe(px, py, color);
            }
        }

        private void SetPixelSafe(int x, int y, Color color)
        {
            if (x >= 0 && x < resolution && y >= 0 && y < resolution)
            {
                texture.SetPixel(x, y, color);
            }
        }

        private void OnDestroy()
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }
    }
}
