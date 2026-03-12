// Folder: UI - HUD/view data only; no kinematics logic.
using System;

namespace KineTutor3D.UI.Data
{
    [Serializable]
    public class MathReadinessQuestion
    {
        public string promptKo = string.Empty;
        public string[] choicesKo = Array.Empty<string>();
        public int correctChoiceIndex;
        public string correctTargetId = string.Empty;
        public string[] correctionMessagesKo = Array.Empty<string>();
        public bool requiresManipulationFirst;
        public float targetAngleDeg = float.NaN;
        public int targetJointIndex;
        public float targetAngleTolerance = 5f;
        public string targetReachGateId = string.Empty;
        public string manipulationInstructionKo = string.Empty;
        public float secondaryTargetAngleDeg = float.NaN;
        public int secondaryTargetJointIndex = -1;
        public float secondaryTargetAngleTolerance = 5f;

        /// <summary>
        /// 런타임 오답 횟수 추적. Serialized가 아닌 런타임 전용 필드.
        /// </summary>
        [NonSerialized] public int attemptCount;

        /// <summary>런타임 오답 카운터를 초기화합니다.</summary>
        public void ResetAttempts()
        {
            attemptCount = 0;
        }
    }
}
