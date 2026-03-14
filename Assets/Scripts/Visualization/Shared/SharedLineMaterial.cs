// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// LineRenderer용 공유 Material 캐시입니다.
    /// Sprites/Default 셰이더를 한 번만 생성하여 재사용합니다.
    /// </summary>
    public static class SharedLineMaterial
    {
        private static Material cached;

        /// <summary>
        /// 공유 Material을 반환합니다. 최초 호출 시 생성됩니다.
        /// </summary>
        public static Material Get()
        {
            if (cached != null)
            {
                return cached;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            cached = new Material(shader)
            {
                name = "KineTutor3D_SharedLine"
            };
            cached.hideFlags = HideFlags.HideAndDontSave;
            return cached;
        }

        /// <summary>
        /// 공유 Material을 기반으로 색상이 설정된 인스턴스를 생성합니다.
        /// MeshRenderer처럼 개별 색상이 필요한 경우에 사용합니다.
        /// </summary>
        public static Material CreateInstance(Color color)
        {
            var baseMat = Get();
            if (baseMat == null)
            {
                return null;
            }

            var instance = new Material(baseMat) { color = color };
            instance.hideFlags = HideFlags.HideAndDontSave;
            return instance;
        }

        /// <summary>
        /// LineRenderer에 공통 기본 설정을 적용합니다.
        /// </summary>
        public static void ConfigureLineRenderer(LineRenderer lr, Color color, float width)
        {
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;
            lr.positionCount = 0;
            lr.numCornerVertices = 2;
            lr.numCapVertices = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.sharedMaterial = Get();
        }
    }
}
