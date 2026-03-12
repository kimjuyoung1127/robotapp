// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// 학습 상호작용 이벤트 타입을 정의합니다.
    /// </summary>
    public enum InteractionType
    {
        Hover,
        Click,
        SliderChange,
        SliderReachTarget,
        MatrixAreaHover,
        StepAction,
        WarmupChoice,
        ReadinessChoice,
        ObserveMotion,
        CompareArc,
        CompareCombination,
        TargetMatch
    }
}

