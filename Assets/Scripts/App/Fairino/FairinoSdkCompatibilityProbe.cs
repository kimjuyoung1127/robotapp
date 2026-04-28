// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO C# SDK가 현재 런타임에서 직접 로드 가능한지 확인합니다.
    /// </summary>
    public static class FairinoSdkCompatibilityProbe
    {
        private const string RobotTypeName = "fairino.Robot";
        private const string PluginRelativePath = "Plugins/Fairino/libfairino.dll";

        public static FairinoSdkCompatibilityReport Probe()
        {
            var report = new FairinoSdkCompatibilityReport
            {
                sdkRuntime = BuildRuntimeLabel(),
                sdkLoadStatus = "not-checked",
                message = string.Empty,
            };

            try
            {
                var assembly = FindLoadedSdkAssembly();
                var dllPath = ResolveDllPath();
                if (assembly == null && File.Exists(dllPath))
                {
                    report.assemblyPath = dllPath;
                    report.managedAssembly = IsManagedAssembly(dllPath);
                    assembly = Assembly.LoadFrom(dllPath);
                }

                if (assembly == null)
                {
                    report.sdkLoadStatus = "sdk-missing";
                    report.message = "SDK 로딩 실패: libfairino.dll을 찾지 못했다. macOS direct가 실패하면 bridge 필요.";
                    return report;
                }

                report.assemblyFound = true;
                report.assemblyName = assembly.GetName().Name;
                if (string.IsNullOrEmpty(report.assemblyPath))
                {
                    report.assemblyPath = assembly.Location ?? string.Empty;
                    report.managedAssembly = string.IsNullOrEmpty(report.assemblyPath) || IsManagedAssembly(report.assemblyPath);
                }

                var robotType = assembly.GetType(RobotTypeName) ?? FindLoadedSdkType();
                if (robotType == null)
                {
                    report.sdkLoadStatus = "robot-type-missing";
                    report.message = "SDK 로딩 실패: fairino.Robot 타입을 찾지 못했다. bridge 필요.";
                    return report;
                }

                report.robotTypeFound = true;
                report.robotTypeName = robotType.FullName;
                var sdkRobot = Activator.CreateInstance(robotType);
                report.robotInstantiated = sdkRobot != null;
                report.versionMethodFound = robotType.GetMethods().Any(m => m.Name == "GetSDKVersion");
                report.sdkVersion = TryReadSdkVersion(sdkRobot);
                report.sdkLoadStatus = report.robotInstantiated ? "direct-ready" : "robot-instantiate-failed";
                report.message = report.robotInstantiated
                    ? "SDK 확인 완료: Mac direct 후보로 사용 가능"
                    : "SDK 로딩 실패: fairino.Robot 인스턴스 생성 실패. bridge 필요.";
                return report;
            }
            catch (Exception ex)
            {
                report.sdkLoadStatus = "sdk-load-failed";
                report.message = $"SDK 로딩 실패: {ex.GetType().Name}: {ex.Message}. bridge 필요.";
                return report;
            }
        }

        private static Assembly FindLoadedSdkAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(asm => asm.GetType(RobotTypeName) != null);
        }

        private static Type FindLoadedSdkType()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(asm => asm.GetType(RobotTypeName))
                .FirstOrDefault(type => type != null);
        }

        private static string ResolveDllPath()
        {
            var dataPath = Application.dataPath;
            return string.IsNullOrEmpty(dataPath)
                ? Path.Combine("Assets", PluginRelativePath)
                : Path.Combine(dataPath, PluginRelativePath);
        }

        private static bool IsManagedAssembly(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string TryReadSdkVersion(object sdkRobot)
        {
            if (sdkRobot == null)
            {
                return string.Empty;
            }

            try
            {
                var method = sdkRobot.GetType().GetMethods().FirstOrDefault(m => m.Name == "GetSDKVersion");
                if (method == null)
                {
                    return string.Empty;
                }

                var args = new object[] { string.Empty };
                var result = method.Invoke(sdkRobot, args);
                var code = result is int intCode ? intCode : result is byte byteCode ? byteCode : -1;
                return code == 0 ? args[0]?.ToString() ?? string.Empty : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string BuildRuntimeLabel()
        {
            return $"{Application.platform} / {RuntimeInformation.OSDescription} / {RuntimeInformation.ProcessArchitecture}";
        }
    }

    [Serializable]
    public sealed class FairinoSdkCompatibilityReport
    {
        public string sdkLoadStatus;
        public string sdkVersion;
        public string sdkRuntime;
        public string assemblyName;
        public string assemblyPath;
        public string robotTypeName;
        public string message;
        public bool assemblyFound;
        public bool managedAssembly;
        public bool robotTypeFound;
        public bool robotInstantiated;
        public bool versionMethodFound;

        public bool IsDirectUsable => assemblyFound && robotTypeFound && robotInstantiated;
    }
}
