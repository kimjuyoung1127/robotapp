// Folder: Visualization - Unity-side rendering and FK binding.
using KineTutor3D.App;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 타깃/성공/경고 마커를 독립적으로 생성하고 표시 상태를 제어합니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class TargetMarkerVisual : MonoBehaviour
    {
        private static readonly string[] TargetPrefabPaths =
        {
            "Assets/Runtime/Prefabs/Teaching/Markers/ShootingTarget.prefab",
            "Assets/Vendors/Archive/GlowingRifts/Shooting Target/ShootingTarget.prefab"
        };

        private static readonly string[] SuccessPrefabPaths =
        {
            "Assets/Runtime/Prefabs/Teaching/Markers/Checkmark_3D_Icon.prefab",
            "Assets/Vendors/Archive/HQPStudios/Low Poly 3D Icons - Pack Lite/Prefabs/Checkmark_3D_Icon.prefab"
        };

        private static readonly string[] WarningPrefabPaths =
        {
            "Assets/Runtime/Prefabs/Teaching/Markers/Warning_3D_Icon.prefab",
            "Assets/Vendors/Archive/HQPStudios/Low Poly 3D Icons - Pack Lite/Prefabs/Warning_3D_Icon.prefab"
        };

        [SerializeField] private AppController appController;
        [SerializeField] private TargetMarkerConfig config = new TargetMarkerConfig();
        [SerializeField] private Transform markerRoot;
        [SerializeField] private GameObject targetMarker;
        [SerializeField] private GameObject successMarker;
        [SerializeField] private GameObject warningMarker;
        [SerializeField] private bool markersVisible;

        private enum MarkerState
        {
            Hidden,
            Target,
            Success,
            Warning
        }

        private MarkerState currentState;

        public GameObject TargetMarker => targetMarker;
        public bool IsMarkersVisible => markersVisible;

        private void Awake()
        {
            EnsureMarkers();
            ApplyState();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Bind(AppController owner)
        {
            Unbind();
            appController = owner;
            if (appController != null)
            {
                appController.OnStepChanged += HandleStepChanged;
            }

            EnsureMarkers();
            ClearFeedback();
        }

        public void SetMarkersVisible(bool visible)
        {
            markersVisible = visible;
            currentState = visible ? MarkerState.Target : MarkerState.Hidden;
            EnsureMarkers();
            ApplyState();
        }

        public void ShowSuccess()
        {
            currentState = markersVisible ? MarkerState.Success : MarkerState.Hidden;
            ApplyState();
        }

        public void ShowWarning()
        {
            currentState = markersVisible ? MarkerState.Warning : MarkerState.Hidden;
            ApplyState();
        }

        public void ClearFeedback()
        {
            currentState = markersVisible ? MarkerState.Target : MarkerState.Hidden;
            ApplyState();
        }

        private void HandleStepChanged(int _step, UI.Data.TutorStepConfig _config)
        {
            ClearFeedback();
        }

        private void EnsureMarkers()
        {
            markerRoot ??= transform.Find("TargetMarkerRoot");
            if (markerRoot == null)
            {
                var root = new GameObject("TargetMarkerRoot");
                root.transform.SetParent(transform, false);
                markerRoot = root.transform;
            }

            markerRoot.localPosition = config.localPosition;
            markerRoot.localScale = config.localScale;

            ResolveCanonicalPrefabs();
            targetMarker ??= EnsureMarkerInstance("TargetMarker", config.targetPrefab, PrimitiveType.Sphere, new Color(0.95f, 0.77f, 0.15f, 1f));
            successMarker ??= EnsureMarkerInstance("TargetSuccess", config.successPrefab, PrimitiveType.Cylinder, new Color(0.28f, 0.84f, 0.46f, 1f));
            warningMarker ??= EnsureMarkerInstance("TargetWarning", config.warningPrefab, PrimitiveType.Cube, new Color(0.93f, 0.37f, 0.28f, 1f));
        }

        private GameObject EnsureMarkerInstance(string name, GameObject prefab, PrimitiveType fallbackPrimitive, Color fallbackColor)
        {
            var existing = markerRoot.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject instance;
            if (prefab != null)
            {
                instance = Instantiate(prefab, markerRoot);
                instance.name = name;
            }
            else
            {
                instance = GameObject.CreatePrimitive(fallbackPrimitive);
                instance.name = name;
                instance.transform.SetParent(markerRoot, false);
                var renderer = instance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = CreateFallbackMarkerMaterial(fallbackColor);
                    renderer.sharedMaterial.color = fallbackColor;
                }

                var collider = instance.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyCollider(collider);
                }
            }

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            return instance;
        }

        private void ResolveCanonicalPrefabs()
        {
#if UNITY_EDITOR
            config.targetPrefab ??= LoadFirstPrefab(TargetPrefabPaths);
            config.successPrefab ??= LoadFirstPrefab(SuccessPrefabPaths);
            config.warningPrefab ??= LoadFirstPrefab(WarningPrefabPaths);
#endif
        }

        private static GameObject LoadFirstPrefab(string[] paths)
        {
            if (paths == null)
            {
                return null;
            }

            for (int i = 0; i < paths.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private void ApplyState()
        {
            if (targetMarker != null)
            {
                targetMarker.SetActive(currentState == MarkerState.Target);
            }

            if (successMarker != null)
            {
                successMarker.SetActive(currentState == MarkerState.Success);
            }

            if (warningMarker != null)
            {
                warningMarker.SetActive(currentState == MarkerState.Warning);
            }
        }

        private void Unbind()
        {
            if (appController != null)
            {
                appController.OnStepChanged -= HandleStepChanged;
            }
        }

        private static void DestroyCollider(Collider collider)
        {
            if (collider == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(collider);
                return;
            }

            DestroyImmediate(collider);
        }

        private static Material CreateFallbackMarkerMaterial(Color fallbackColor)
        {
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader)
            {
                name = "KineTutor3D_TargetMarkerFallback"
            };
            material.color = fallbackColor;
            return material;
        }
    }
}
