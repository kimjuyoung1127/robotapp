// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// 용어 사전 항목 1건을 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "KineTutor3D/UI/Glossary Entry", fileName = "GlossaryEntry")]
    public class GlossaryEntryConfig : ScriptableObject
    {
        public string symbol = string.Empty;
        public string koreanName = string.Empty;
        [TextArea(2, 5)] public string easyDescription = string.Empty;
        [TextArea(2, 5)] public string mathDescription = string.Empty;
        public int[] relatedSteps = new int[0];
    }
}

