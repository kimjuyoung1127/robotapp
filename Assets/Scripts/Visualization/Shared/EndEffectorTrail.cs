// Folder: Visualization - Unity-side rendering and FK binding.
using KineTutor3D.App;
using KineTutor3D.Math;
using UnityEngine;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// AppController 기반 씬(Main, MathReadiness, Sandbox)에서
    /// EETrailRenderer를 AppController 이벤트에 바인딩하는 어댑터입니다.
    /// 궤적 렌더링은 EETrailRenderer에 위임합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class EndEffectorTrail : MonoBehaviour
    {
        [SerializeField] private AppController appController;
        [SerializeField] private Transform trackedTransform;
        [SerializeField] private bool trailVisible;

        private EETrailRenderer trailRenderer;

        /// <summary>
        /// 현재 기록된 궤적 포인트 수입니다.
        /// </summary>
        public int PointCount => trailRenderer != null ? trailRenderer.PointCount : 0;

        /// <summary>
        /// 트레일 가시성 상태입니다.
        /// </summary>
        public bool IsTrailVisible => trailVisible;

        private void Awake()
        {
            EnsureTrailRenderer();
        }

        private void OnEnable()
        {
            EnsureTrailRenderer();
        }

        private void OnDisable()
        {
            Unbind();
        }

        /// <summary>
        /// AppController에 바인딩하여 kinematics 이벤트를 수신합니다.
        /// </summary>
        public void Bind(AppController owner)
        {
            Unbind();
            appController = owner;
            trackedTransform ??= transform;

            if (appController != null)
            {
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
                appController.OnStepChanged += HandleStepChanged;
            }

            EnsureTrailRenderer();
            SetTrailVisible(trailVisible);
        }

        /// <summary>
        /// 트레일 가시성을 설정합니다.
        /// </summary>
        public void SetTrailVisible(bool visible)
        {
            trailVisible = visible;
            if (trailRenderer != null)
            {
                trailRenderer.SetVisible(visible);
            }
        }

        /// <summary>
        /// 트레일을 초기화합니다.
        /// </summary>
        public void ClearTrail()
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        private void HandleKinematicsUpdated(Mat4D _a1, Mat4D _a2, Mat4D _t02, TutorPose _pose)
        {
            if (!trailVisible || appController == null
                || appController.LastUpdateCause != RuntimeUpdateCause.JointAngleChange)
            {
                return;
            }

            if (trackedTransform == null || trailRenderer == null)
            {
                return;
            }

            trailRenderer.AddWorldPoint(trackedTransform.position);
        }

        private void HandleStepChanged(int _step, UI.Data.TutorStepConfig _config)
        {
            ClearTrail();
        }

        private void EnsureTrailRenderer()
        {
            if (trailRenderer != null)
            {
                return;
            }

            trailRenderer = GetComponent<EETrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<EETrailRenderer>();
            }
        }

        private void Unbind()
        {
            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController.OnStepChanged -= HandleStepChanged;
            }
        }
    }
}
