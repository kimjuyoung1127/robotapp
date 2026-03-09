// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Slider 媛?蹂寃쎌쓣 寃뚯씠???곹샇?묒슜 ?대깽?몃줈 ?꾨떖?⑸땲??
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

