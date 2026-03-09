// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// 온보딩 시퀀스 타임라인 데이터를 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "KineTutor3D/UI/Onboarding Sequence", fileName = "OnboardingSequence")]
    public class OnboardingSequenceConfig : ScriptableObject
    {
        public OnboardingStepEvent[] events = Array.Empty<OnboardingStepEvent>();
    }

    /// <summary>
    /// 온보딩 단계 이벤트 한 건입니다.
    /// </summary>
    [Serializable]
    public class OnboardingStepEvent
    {
        public float delaySeconds = 0.5f;
        public OnboardingTarget target = OnboardingTarget.None;
        [TextArea(2, 5)] public string messageKo = string.Empty;
    }

    /// <summary>
    /// 온보딩 스포트라이트 타겟입니다.
    /// </summary>
    public enum OnboardingTarget
    {
        None,
        RobotViewport,
        DHTable,
        DHCell
    }
}

