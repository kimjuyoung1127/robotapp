// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FR5 프리셋 포즈 정의입니다.
    /// Home(영점), Ready(작업 준비), Folded(접힘) 포즈를 제공합니다.
    /// </summary>
    public static class FR5PosePresets
    {
        /// <summary>
        /// 프리셋 포즈 구조체입니다.
        /// </summary>
        public readonly struct Preset
        {
            /// <summary>
            /// 프리셋 이름입니다.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// 프리셋 설명입니다.
            /// </summary>
            public string Description { get; }

            private readonly double[] jointAnglesDeg;

            /// <summary>
            /// 6축 관절 각도 (도 단위) 복사본을 반환합니다.
            /// </summary>
            public double[] JointAnglesDeg => (double[])jointAnglesDeg.Clone();

            /// <summary>
            /// 프리셋을 생성합니다.
            /// </summary>
            public Preset(string name, string description, double[] jointAnglesDeg)
            {
                Name = name ?? string.Empty;
                Description = description ?? string.Empty;
                this.jointAnglesDeg = jointAnglesDeg != null
                    ? (double[])jointAnglesDeg.Clone()
                    : new double[6];
            }
        }

        /// <summary>
        /// Home 포즈 (모든 관절 0도)입니다.
        /// </summary>
        public static readonly Preset Home = new Preset(
            "Home",
            "모든 관절 영점 (0°)",
            new double[] { 0, 0, 0, 0, 0, 0 });

        /// <summary>
        /// Ready 포즈 (작업 준비 자세)입니다.
        /// </summary>
        public static readonly Preset Ready = new Preset(
            "Ready",
            "작업 준비 자세",
            new double[] { 0, -90, 90, 0, 90, 0 });

        /// <summary>
        /// Folded 포즈 (접힌 자세)입니다.
        /// </summary>
        public static readonly Preset Folded = new Preset(
            "Folded",
            "접힌 자세 (보관용)",
            new double[] { 0, -160, 150, 0, 10, 0 });

        /// <summary>
        /// 마지막 Sync 스냅샷 기반 동적 프리셋입니다.
        /// </summary>
        private static Preset current = new Preset(
            "Current",
            "마지막 동기화 스냅샷",
            new double[] { 0, 0, 0, 0, 0, 0 });

        /// <summary>
        /// Current 프리셋을 반환합니다.
        /// </summary>
        public static Preset Current => current;

        private static Preset[] cachedAll;

        /// <summary>
        /// Current 프리셋의 관절 각도를 업데이트합니다.
        /// </summary>
        public static void UpdateCurrent(double[] jointAnglesDeg)
        {
            if (jointAnglesDeg != null && jointAnglesDeg.Length >= 6)
            {
                current = new Preset("Current", "마지막 동기화 스냅샷", jointAnglesDeg);
                cachedAll = null;
            }
        }

        /// <summary>
        /// 모든 프리셋 목록입니다.
        /// </summary>
        public static Preset[] All
        {
            get
            {
                if (cachedAll == null)
                {
                    cachedAll = new[] { Home, Ready, Folded, current };
                }

                return cachedAll;
            }
        }
    }
}
