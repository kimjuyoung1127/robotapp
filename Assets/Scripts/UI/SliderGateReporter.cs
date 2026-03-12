// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Slider 값 변경을 게이트 상호작용 이벤트로 전달합니다.
    /// </summary>
    public class SliderGateReporter : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private string targetId = "joint_slider_1";

        private void Awake()
        {
            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }

            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        private void OnValueChanged(float _)
        {
            var app = FindAnyObjectByType<AppController>();
            app?.ReportInteraction(InteractionType.SliderChange, targetId);
        }
    }
}

