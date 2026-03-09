// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// 전체 용어 사전을 보관하는 데이터베이스입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "KineTutor3D/UI/Glossary Database", fileName = "GlossaryDatabase")]
    public class GlossaryDatabase : ScriptableObject
    {
        public GlossaryEntryConfig[] entries = new GlossaryEntryConfig[0];
    }
}

