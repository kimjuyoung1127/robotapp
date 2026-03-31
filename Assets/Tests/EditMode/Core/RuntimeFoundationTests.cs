// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RuntimeFoundation 동작을 검증하는 EditMode 테스트입니다.
using System;
using System.Reflection;
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.Templates;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Validates Phase 5 runtime foundation state capture behavior.
    /// </summary>
    public class RuntimeFoundationTests
    {
        private const double Tolerance = 1e-9;

        [Test]
        public void SetJointAngleDegrees_CapturesPreviousJointState_AndCause()
        {
            var service = CreateRuntimeService();
            ApplyTemplate(service);
            Invoke(service, "SetJointAngleDegrees", 0, 30f, null, null);

            var beforeSecondChange = GetField<Mat4D>(GetState(service), "CurrentEndEffectorTransform");

            Invoke(service, "SetJointAngleDegrees", 0, 45f, null, null);

            var state = GetState(service);
            var previousJointValues = GetField<double[]>(state, "PreviousJointValuesRad");
            var currentJointValues = GetField<double[]>(state, "CurrentJointValuesRad");
            var previousTransform = GetField<Mat4D>(state, "PreviousEndEffectorTransform");
            var cause = GetField<RuntimeUpdateCause>(state, "LastUpdateCause");
            var changedJointIndex = GetField<int>(state, "ChangedJointIndex");

            Assert.That(previousJointValues[0], Is.EqualTo(DegreesToRadians(30.0)).Within(Tolerance));
            Assert.That(currentJointValues[0], Is.EqualTo(DegreesToRadians(45.0)).Within(Tolerance));
            MatrixAssert.AreEqual(beforeSecondChange, previousTransform, Tolerance, "Second joint change should preserve the previous EE transform.");
            Assert.That(cause, Is.EqualTo(RuntimeUpdateCause.JointAngleChange));
            Assert.That(changedJointIndex, Is.EqualTo(0));
        }

        [Test]
        public void TrySetDhParameter_CapturesPreviousTransform_AndDhEditCause()
        {
            var service = CreateRuntimeService();
            ApplyTemplate(service);

            var currentTransform = GetField<Mat4D>(GetState(service), "CurrentEndEffectorTransform");
            var args = new object[] { 0, DhEditableField.A, 1.5, null };
            var success = (bool)Invoke(service, "TrySetDhParameter", args);

            Assert.That(success, Is.True);
            Assert.That(args[3], Is.EqualTo(string.Empty));

            var state = GetState(service);
            var previousTransform = GetField<Mat4D>(state, "PreviousEndEffectorTransform");
            var cause = GetField<RuntimeUpdateCause>(state, "LastUpdateCause");
            var changedJointIndex = GetField<int>(state, "ChangedJointIndex");

            MatrixAssert.AreEqual(currentTransform, previousTransform, Tolerance, "DH edits should preserve the pre-edit EE transform.");
            Assert.That(cause, Is.EqualTo(RuntimeUpdateCause.DhParameterEdit));
            Assert.That(changedJointIndex, Is.EqualTo(0));
        }

        [Test]
        public void ApplyTemplate_MarksTemplateApply_AndRetainsPreviousJointSnapshot()
        {
            var service = CreateRuntimeService();
            ApplyTemplate(service);
            Invoke(service, "SetJointAngleDegrees", 0, 45f, null, null);

            ApplyTemplate(service);

            var state = GetState(service);
            var previousJointValues = GetField<double[]>(state, "PreviousJointValuesRad");
            var cause = GetField<RuntimeUpdateCause>(state, "LastUpdateCause");
            var changedJointIndex = GetField<int>(state, "ChangedJointIndex");

            Assert.That(previousJointValues[0], Is.EqualTo(DegreesToRadians(45.0)).Within(Tolerance));
            Assert.That(cause, Is.EqualTo(RuntimeUpdateCause.TemplateApply));
            Assert.That(changedJointIndex, Is.EqualTo(-1));
        }

        private static object CreateRuntimeService()
        {
            var type = typeof(AppController).Assembly.GetType("KineTutor3D.App.KinematicsRuntimeService", throwOnError: true);
            return Activator.CreateInstance(type, nonPublic: true);
        }

        private static void ApplyTemplate(object service)
        {
            Invoke(service, "ApplyTemplate", Template2DOF_RR.Create(), null, null);
        }

        private static object GetState(object service)
        {
            return service.GetType().GetProperty("State", BindingFlags.Instance | BindingFlags.Public)?.GetValue(service);
        }

        private static T GetField<T>(object target, string name)
        {
            var type = target.GetType();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return (T)field.GetValue(target);
            }

            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                return (T)property.GetValue(target);
            }

            throw new MissingMemberException(type.FullName, name);
        }

        private static object Invoke(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return method?.Invoke(target, args);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (System.Math.PI / 180.0);
        }
    }
}
