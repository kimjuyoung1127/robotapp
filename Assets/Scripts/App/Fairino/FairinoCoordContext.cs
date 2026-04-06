// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 컨트롤러의 현재 tool/user 좌표 문맥을 나타냅니다.
    /// </summary>
    public readonly struct FairinoCoordContext
    {
        /// <summary>
        /// 활성 TCP(tool) ID입니다.
        /// </summary>
        public int ToolId { get; }

        /// <summary>
        /// 활성 user/workobject ID입니다.
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// 현재 tool 좌표 포즈입니다.
        /// </summary>
        public double[] ToolPose { get; }

        /// <summary>
        /// 현재 workobject 좌표 포즈입니다.
        /// </summary>
        public double[] WObjPose { get; }

        public FairinoCoordContext(int toolId, int userId, double[] toolPose, double[] wObjPose)
        {
            ToolId = toolId;
            UserId = userId;
            ToolPose = toolPose != null ? (double[])toolPose.Clone() : new double[6];
            WObjPose = wObjPose != null ? (double[])wObjPose.Clone() : new double[6];
        }

        public static FairinoCoordContext Default()
        {
            return new FairinoCoordContext(0, 0, new double[6], new double[6]);
        }
    }
}
