// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 컨트롤러 fault/safety 상태를 표현합니다.
    /// </summary>
    public readonly struct FairinoControllerFault
    {
        /// <summary>
        /// 메인 에러 코드입니다.
        /// </summary>
        public int MainCode { get; }

        /// <summary>
        /// 서브 에러 코드입니다.
        /// </summary>
        public int SubCode { get; }

        /// <summary>
        /// Safety stop 상태 여부입니다.
        /// </summary>
        public bool IsSafetyStop { get; }

        public bool HasBlockingFault => MainCode != 0 || SubCode != 0 || IsSafetyStop;

        public FairinoControllerFault(int mainCode, int subCode, bool isSafetyStop)
        {
            MainCode = mainCode;
            SubCode = subCode;
            IsSafetyStop = isSafetyStop;
        }

        public static FairinoControllerFault None()
        {
            return new FairinoControllerFault(0, 0, false);
        }
    }
}
