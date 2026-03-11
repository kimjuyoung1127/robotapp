// Folder: Visualization - Unity-side rendering and FK binding.
using System;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 타깃/성공/경고 마커의 프리팹과 배치 기준점을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TargetMarkerConfig
    {
        public GameObject targetPrefab;
        public GameObject successPrefab;
        public GameObject warningPrefab;
        public Vector3 localPosition = new Vector3(1.25f, 0.1f, 0.7f);
        public Vector3 localScale = new Vector3(0.35f, 0.35f, 0.35f);
    }
}
