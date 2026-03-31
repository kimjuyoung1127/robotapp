// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.Math;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 관절 변화에 대한 스냅샷 상태를 담는 순수 C# 클래스입니다.
    /// </summary>
    public sealed class WhyItMovedState
    {
        public RuntimeUpdateCause UpdateCause { get; private set; }
        public int ChangedJointIndex { get; private set; } = -1;
        public double PreviousValueRad { get; private set; }
        public double CurrentValueRad { get; private set; }
        public double DeltaDeg { get; private set; }
        public Vec3D PreviousEEPosition { get; private set; }
        public Vec3D CurrentEEPosition { get; private set; }
        public Vec3D EEDisplacement { get; private set; }
        public double EEDistanceMoved { get; private set; }
        public string[] AffectedLinkNames { get; private set; } = System.Array.Empty<string>();
        public bool IsMeaningfulChange { get; private set; }

        private const double MeaningfulThresholdDeg = 0.01;

        /// <summary>
        /// AppController 상태로부터 WhyItMovedState를 계산합니다.
        /// </summary>
        public void Compute(
            RuntimeUpdateCause cause,
            int changedJointIndex,
            double[] previousJointValuesRad,
            double[] currentJointValuesRad,
            Vec3D previousEEPosition,
            Vec3D currentEEPosition,
            int jointCount)
        {
            UpdateCause = cause;
            ChangedJointIndex = changedJointIndex;
            PreviousEEPosition = previousEEPosition;
            CurrentEEPosition = currentEEPosition;
            EEDisplacement = currentEEPosition - previousEEPosition;
            EEDistanceMoved = EEDisplacement.Magnitude();

            if (cause != RuntimeUpdateCause.JointAngleChange
                || changedJointIndex < 0
                || previousJointValuesRad == null
                || currentJointValuesRad == null
                || changedJointIndex >= previousJointValuesRad.Length
                || changedJointIndex >= currentJointValuesRad.Length)
            {
                PreviousValueRad = 0.0;
                CurrentValueRad = 0.0;
                DeltaDeg = 0.0;
                AffectedLinkNames = System.Array.Empty<string>();
                IsMeaningfulChange = false;
                return;
            }

            PreviousValueRad = previousJointValuesRad[changedJointIndex];
            CurrentValueRad = currentJointValuesRad[changedJointIndex];
            DeltaDeg = (CurrentValueRad - PreviousValueRad) * (180.0 / System.Math.PI);

            IsMeaningfulChange = System.Math.Abs(DeltaDeg) > MeaningfulThresholdDeg;

            // Affected links: from changed joint index onward to EE
            var affected = new System.Collections.Generic.List<string>();
            for (int i = changedJointIndex; i < jointCount; i++)
            {
                affected.Add($"Link{i}");
            }
            affected.Add("EE");
            AffectedLinkNames = affected.ToArray();
        }
    }
}
