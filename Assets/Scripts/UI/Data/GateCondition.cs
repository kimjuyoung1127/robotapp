// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// 스텝 진행 게이트 조건 한 건을 정의합니다.
    /// </summary>
    [Serializable]
    public class GateCondition
    {
        public InteractionType interactionType = InteractionType.Hover;
        public string targetId = string.Empty;
        public int requiredCount = 1;

        public string GetKey()
        {
            return $"{interactionType}:{targetId}";
        }
    }
}

