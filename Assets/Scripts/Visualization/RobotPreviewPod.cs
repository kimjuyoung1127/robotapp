// Folder: Visualization - 단일 로봇 3D 프리뷰 유닛.
using KineTutor3D.Types;
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 단일 로봇의 3D 프리뷰를 관리하는 컴포넌트입니다.
    /// 페데스탈, 이름 라벨, 턴테이블 회전을 포함합니다.
    /// </summary>
    public class RobotPreviewPod : MonoBehaviour
    {
        private const float TurntableSpeed = 15f;
        private const float SelectedScale = 1.10f;
        private const float PreviewScale = 0.90f;

        private RobotMetadataInfo metadata;
        private GameObject meshRoot;
        private GameObject pedestal;
        private TextMesh nameLabel;
        private BoxCollider hitCollider;
        private bool selected;
        private bool meshVisible = true;
        private bool initialized;

        /// <summary>로봇 ID를 반환합니다.</summary>
        public string RobotId => initialized ? metadata.RobotId : string.Empty;

        /// <summary>선택 상태를 반환합니다.</summary>
        public bool IsSelected => selected;

        /// <summary>
        /// pod를 초기화합니다.
        /// </summary>
        public void Initialize(RobotMetadataInfo meta, GameObject createdMeshRoot, bool showLabel)
        {
            metadata = meta;
            meshRoot = createdMeshRoot;
            initialized = true;

            if (meshRoot != null)
            {
                meshRoot.transform.SetParent(transform, false);
            }

            BuildPedestal();

            if (showLabel)
            {
                BuildNameLabel();
            }

            EnsureHitCollider();
            SetSelected(false);
        }

        /// <summary>
        /// 선택 상태를 설정합니다. 선택 시 스케일 확대 및 턴테이블 활성화.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            selected = isSelected;
            float s = isSelected ? SelectedScale : PreviewScale;
            transform.localScale = new Vector3(s, s, s);
        }

        /// <summary>
        /// 관절 포즈를 설정합니다 (정적 프리뷰용).
        /// </summary>
        public void SetPose(double[] jointsDeg)
        {
            if (meshRoot == null || jointsDeg == null)
            {
                return;
            }

            for (int i = 0; i < jointsDeg.Length; i++)
            {
                var pivot = meshRoot.transform.Find($"JointPivot{i}");
                if (pivot == null)
                {
                    continue;
                }

                pivot.localRotation = Quaternion.Euler(0f, 0f, (float)jointsDeg[i]);
            }
        }

        /// <summary>
        /// 메시 렌더러 활성화/비활성화 (viewport 풀링용).
        /// </summary>
        public void SetMeshVisible(bool visible)
        {
            meshVisible = visible;
            if (meshRoot == null)
            {
                return;
            }

            foreach (var renderer in meshRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.enabled = visible;
            }
        }

        /// <summary>
        /// pod를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (meshRoot != null)
            {
                SafeDestroy(meshRoot);
                meshRoot = null;
            }

            if (pedestal != null)
            {
                SafeDestroy(pedestal);
                pedestal = null;
            }

            initialized = false;
            SafeDestroy(gameObject);
        }

        private void Update()
        {
            if (!selected || meshRoot == null)
            {
                return;
            }

            meshRoot.transform.Rotate(Vector3.up, TurntableSpeed * Time.deltaTime, Space.Self);
        }

        private void BuildPedestal()
        {
            pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(transform, false);

            float radius = UIDesignTokens.Size.PedestalRadius;
            float height = UIDesignTokens.Size.PedestalHeight;
            pedestal.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            pedestal.transform.localPosition = new Vector3(0f, -height * 0.5f, 0f);

            var renderer = pedestal.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = UIDesignTokens.Colors.PedestalSurface
                };
            }

            var collider = pedestal.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private void BuildNameLabel()
        {
            var labelGo = new GameObject("NameLabel");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, -0.15f, 0f);

            nameLabel = labelGo.AddComponent<TextMesh>();
            nameLabel.text = metadata.DisplayName;
            nameLabel.fontSize = 32;
            nameLabel.characterSize = 0.02f;
            nameLabel.anchor = TextAnchor.UpperCenter;
            nameLabel.alignment = TextAlignment.Center;
            nameLabel.color = UIDesignTokens.Colors.TextPrimary;
        }

        private void EnsureHitCollider()
        {
            hitCollider = GetComponent<BoxCollider>();
            if (hitCollider == null)
            {
                hitCollider = gameObject.AddComponent<BoxCollider>();
            }

            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                hitCollider.center = Vector3.zero;
                hitCollider.size = Vector3.one;
                return;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var localCenter = transform.InverseTransformPoint(bounds.center);
            var scaledSize = new Vector3(
                bounds.size.x / Mathf.Max(0.0001f, transform.lossyScale.x),
                bounds.size.y / Mathf.Max(0.0001f, transform.lossyScale.y),
                bounds.size.z / Mathf.Max(0.0001f, transform.lossyScale.z));

            hitCollider.center = localCenter;
            hitCollider.size = scaledSize + new Vector3(0.2f, 0.2f, 0.2f);
        }

        private static void SafeDestroy(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
