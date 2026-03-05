using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 스텝 목표/힌트/게이트 진행 텍스트를 표시합니다.
    /// </summary>
    public class StepTutorPanel : MonoBehaviour
    {
        [SerializeField] private Text stepTitleText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text gateProgressText;

        private void Awake()
        {
            if (stepTitleText == null)
            {
                var go = GameObject.Find("StepTitleText");
                if (go != null) stepTitleText = go.GetComponent<Text>();
            }

            if (objectiveText == null)
            {
                var go = GameObject.Find("StepObjectiveText");
                if (go != null) objectiveText = go.GetComponent<Text>();
            }

            if (hintText == null)
            {
                var go = GameObject.Find("StepHintText");
                if (go != null) hintText = go.GetComponent<Text>();
            }

            if (gateProgressText == null)
            {
                var go = GameObject.Find("GateProgressText");
                if (go != null) gateProgressText = go.GetComponent<Text>();
            }
        }

        public void ApplyStep(TutorStepConfig config, int currentStep, int totalSteps, bool gateSatisfied, string gateProgress)
        {
            if (config == null)
            {
                return;
            }

            if (stepTitleText != null)
            {
                stepTitleText.text = $"Step {currentStep}/{totalSteps}: {config.stepTitleKo}";
            }

            if (objectiveText != null)
            {
                objectiveText.text = config.objectiveKo;
            }

            if (hintText != null)
            {
                hintText.text = config.hintKo;
            }

            UpdateGateState(gateSatisfied, gateProgress);
        }

        public void UpdateGateState(bool gateSatisfied, string gateProgress)
        {
            if (gateProgressText == null)
            {
                return;
            }

            gateProgressText.text = gateSatisfied ? "다음 단계로 이동할 수 있습니다." : gateProgress;
        }
    }
}
