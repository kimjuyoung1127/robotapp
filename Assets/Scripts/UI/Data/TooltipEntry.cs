// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// 스텝별 툴팁 한 건의 내용을 정의합니다.
    /// </summary>
    [Serializable]
    public class TooltipEntry
    {
        public string anchorId = string.Empty;
        public string titleKo = string.Empty;
        [TextArea(2, 6)] public string bodyKo = string.Empty;
        public bool autoShow;
        public int step = 1;
    }
}

