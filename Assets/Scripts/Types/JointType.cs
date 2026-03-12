// Folder: Types - Domain value types; no UnityEngine references.
namespace KineTutor3D.Types
{
    /// <summary>
    /// 관절의 구동 타입을 정의합니다.
    /// </summary>
    public enum JointType
    {
        /// <summary>
        /// 회전 관절입니다.
        /// </summary>
        Revolute = 0,

        /// <summary>
        /// 직선 이동 관절입니다.
        /// </summary>
        Prismatic = 1
    }
}
