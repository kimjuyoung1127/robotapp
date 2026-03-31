// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.Kinematics;
using KineTutor3D.Math;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.App
{
    internal sealed class KinematicsRuntimeService
    {
        private readonly KinematicsRuntimeState state = new KinematicsRuntimeState();

        public KinematicsRuntimeState State => state;

        public JointLimit GetJointLimit(int jointIndex)
        {
            if (state.CurrentTemplate == null)
            {
                return new JointLimit(-System.Math.PI, System.Math.PI);
            }

            return state.CurrentTemplate.GetJointLimit(jointIndex);
        }

        public double GetJointAngleDegrees(int jointIndex)
        {
            if (jointIndex < 0 || jointIndex >= state.CurrentJointValuesRad.Length)
            {
                return 0.0;
            }

            return state.CurrentJointValuesRad[jointIndex] * Mathf.Rad2Deg;
        }

        public void ApplyTemplate(RobotTemplate template, Slider jointSlider1, Slider jointSlider2)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            CapturePreviousState(-1, RuntimeUpdateCause.TemplateApply);
            state.CurrentTemplate = template;
            state.CurrentLinks = template.GetLinks();
            state.CurrentJointValuesRad = new double[template.Dof];
            ConfigureJointSlider(0, jointSlider1);
            ConfigureJointSlider(1, jointSlider2);
            RecomputeForwardKinematics();
        }

        public void SetJointAngleDegrees(int jointIndex, float degrees, Slider jointSlider1, Slider jointSlider2)
        {
            if (state.CurrentTemplate == null)
            {
                return;
            }

            if (jointIndex < 0 || jointIndex >= state.CurrentJointValuesRad.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(jointIndex));
            }

            if (float.IsNaN(degrees) || float.IsInfinity(degrees))
            {
                throw new ArgumentException($"유효하지 않은 관절 각도입니다: {degrees}", nameof(degrees));
            }

            CapturePreviousState(jointIndex, RuntimeUpdateCause.JointAngleChange);
            state.CurrentJointValuesRad[jointIndex] = DegreesToRadians(degrees);
            SetSliderValueWithoutCallback(jointIndex, degrees, jointSlider1, jointSlider2);
            RecomputeForwardKinematics();
        }

        public bool TrySetDhParameter(int linkIndex, DhEditableField field, double value, out string error)
        {
            error = string.Empty;

            if (state.CurrentTemplate == null || state.CurrentLinks == null || state.CurrentLinks.Length == 0)
            {
                error = "템플릿이 초기화되지 않았습니다.";
                return false;
            }

            if (linkIndex < 0 || linkIndex >= state.CurrentLinks.Length)
            {
                error = $"유효하지 않은 링크 인덱스입니다: {linkIndex}";
                return false;
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                error = "NaN/Infinity는 허용되지 않습니다.";
                return false;
            }

            if (field == DhEditableField.Theta)
            {
                error = "theta는 읽기전용입니다. 슬라이더를 사용하세요.";
                return false;
            }

            var link = state.CurrentLinks[linkIndex];
            DHLink updated;

            switch (field)
            {
                case DhEditableField.D:
                    updated = new DHLink(link.Theta, value, link.A, link.Alpha, link.JointType);
                    break;
                case DhEditableField.A:
                    updated = new DHLink(link.Theta, link.D, value, link.Alpha, link.JointType);
                    break;
                case DhEditableField.Alpha:
                    updated = new DHLink(link.Theta, link.D, link.A, value, link.JointType);
                    break;
                default:
                    error = "지원하지 않는 DH 필드입니다.";
                    return false;
            }

            CapturePreviousState(linkIndex, RuntimeUpdateCause.DhParameterEdit);
            state.CurrentLinks[linkIndex] = updated;
            RecomputeForwardKinematics();
            return true;
        }

        public void HandleJointSliderChanged(int jointIndex, float valueDegrees)
        {
            if (state.CurrentTemplate == null || jointIndex >= state.CurrentJointValuesRad.Length)
            {
                return;
            }

            CapturePreviousState(jointIndex, RuntimeUpdateCause.JointAngleChange);
            state.CurrentJointValuesRad[jointIndex] = DegreesToRadians(valueDegrees);
            RecomputeForwardKinematics();
        }

        private void CapturePreviousState(int changedJointIndex, RuntimeUpdateCause cause)
        {
            state.PreviousJointValuesRad = (double[])state.CurrentJointValuesRad.Clone();
            state.PreviousEndEffectorPose = state.CurrentEndEffectorPose;
            state.PreviousEndEffectorPosition = state.CurrentEndEffectorTransform.ExtractPosition();
            state.PreviousEndEffectorTransform = state.CurrentEndEffectorTransform;
            state.ChangedJointIndex = changedJointIndex;
            state.LastUpdateCause = cause;
        }

        private void ConfigureJointSlider(int jointIndex, Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            if (state.CurrentTemplate == null || jointIndex >= state.CurrentTemplate.Dof)
            {
                slider.gameObject.SetActive(false);
                return;
            }

            slider.gameObject.SetActive(true);
            var limit = state.CurrentTemplate.GetJointLimit(jointIndex);
            var minDeg = (float)(limit.Min * Mathf.Rad2Deg);
            var maxDeg = (float)(limit.Max * Mathf.Rad2Deg);
            slider.minValue = minDeg;
            slider.maxValue = maxDeg;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(0f);
            state.CurrentJointValuesRad[jointIndex] = 0.0;
        }

        private void RecomputeForwardKinematics()
        {
            if (state.CurrentLinks == null || state.CurrentLinks.Length == 0 || state.CurrentJointValuesRad == null || state.CurrentJointValuesRad.Length == 0)
            {
                state.CurrentA1 = Mat4D.Identity;
                state.CurrentA2 = Mat4D.Identity;
                state.CurrentT02 = Mat4D.Identity;
                state.CurrentEndEffectorTransform = Mat4D.Identity;
                state.CurrentEndEffectorPose = new TutorPose(state.CurrentEndEffectorTransform.ExtractPosition(), state.CurrentEndEffectorTransform.ExtractRotation());
                return;
            }

            state.CurrentA1 = state.CurrentLinks.Length > 0 ? DHStandard.ComputeA(state.CurrentLinks[0], state.CurrentJointValuesRad[0]) : Mat4D.Identity;
            state.CurrentA2 = state.CurrentLinks.Length > 1 ? DHStandard.ComputeA(state.CurrentLinks[1], state.CurrentJointValuesRad[1]) : Mat4D.Identity;
            var all = ForwardKinematics.ComputeAll(state.CurrentLinks, state.CurrentJointValuesRad);
            state.CurrentEndEffectorTransform = all[all.Length - 1];
            state.CurrentT02 = all[System.Math.Min(1, all.Length - 1)];
            state.CurrentEndEffectorPose = new TutorPose(state.CurrentEndEffectorTransform.ExtractPosition(), state.CurrentEndEffectorTransform.ExtractRotation());
        }

        private static void SetSliderValueWithoutCallback(int jointIndex, float valueDegrees, Slider jointSlider1, Slider jointSlider2)
        {
            var slider = jointIndex switch
            {
                0 => jointSlider1,
                1 => jointSlider2,
                _ => null
            };

            if (slider == null)
            {
                return;
            }

            slider.SetValueWithoutNotify(valueDegrees);
        }

        private static double DegreesToRadians(float degrees)
        {
            return degrees * (System.Math.PI / 180.0);
        }
    }
}
