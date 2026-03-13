// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using System.Collections.Generic;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 스텝별 게이트 조건을 평가하고 Next 버튼 상태를 제어합니다.
    /// </summary>
    public class InteractionGateController : MonoBehaviour
    {
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private bool hardGateEnabled = true;

        private readonly Dictionary<string, int> countByKey = new Dictionary<string, int>();
        private GateCondition[] conditions = Array.Empty<GateCondition>();
        private string completionMessage = string.Empty;

        public event Action<bool, string> GateStateChanged;
        public bool IsGateSatisfied { get; private set; }

        private void Awake()
        {
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (nextButton == null && canvas != null)
            {
                foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (btn.gameObject.name == "BtnNext") { nextButton = btn; break; }
                }
            }

            if (skipButton == null && canvas != null)
            {
                foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (btn.gameObject.name == "BtnSkip") { skipButton = btn; break; }
                }
            }
        }

        public void LoadStep(TutorStepConfig config, int stepNumber)
        {
            countByKey.Clear();
            conditions = config != null && config.conditions != null ? config.conditions : Array.Empty<GateCondition>();
            completionMessage = config != null ? config.gateMeetToastKo : string.Empty;

            IsGateSatisfied = conditions.Length == 0;
            ApplyButtonState();
            GateStateChanged?.Invoke(IsGateSatisfied, string.Empty);
        }

        public void RegisterInteraction(InteractionType interactionType, string targetId)
        {
            var key = $"{interactionType}:{targetId}";
            countByKey.TryGetValue(key, out var current);
            countByKey[key] = current + 1;

            var before = IsGateSatisfied;
            IsGateSatisfied = Evaluate();
            ApplyButtonState();

            if (!before && IsGateSatisfied)
            {
                GateStateChanged?.Invoke(true, completionMessage);
            }
            else if (before != IsGateSatisfied)
            {
                GateStateChanged?.Invoke(IsGateSatisfied, string.Empty);
            }
        }

        public void SkipCurrentGate()
        {
            IsGateSatisfied = true;
            ApplyButtonState();
            GateStateChanged?.Invoke(true, string.Empty);
        }

        public string GetProgressText()
        {
            if (conditions.Length == 0)
            {
                return "게이트 조건 없음";
            }

            var met = 0;
            for (var i = 0; i < conditions.Length; i++)
            {
                if (ConditionMet(conditions[i])) met++;
            }

            return $"게이트 진행: {met}/{conditions.Length}";
        }

        private bool Evaluate()
        {
            if (conditions.Length == 0) return true;
            for (var i = 0; i < conditions.Length; i++)
            {
                if (!ConditionMet(conditions[i])) return false;
            }

            return true;
        }

        private bool ConditionMet(GateCondition condition)
        {
            if (condition == null) return true;
            var key = condition.GetKey();
            countByKey.TryGetValue(key, out var current);
            return current >= Mathf.Max(1, condition.requiredCount);
        }

        private void ApplyButtonState()
        {
            if (nextButton != null) nextButton.interactable = !hardGateEnabled || IsGateSatisfied;
            if (skipButton != null) skipButton.gameObject.SetActive(hardGateEnabled && !IsGateSatisfied);
        }
    }
}

