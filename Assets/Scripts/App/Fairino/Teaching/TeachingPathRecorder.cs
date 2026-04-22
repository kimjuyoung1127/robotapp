// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections.Generic;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Runtime readback/current pose samples을 웨이포인트 시퀀스로 변환하는 티칭 경로 기록기입니다.
    /// </summary>
    public sealed class TeachingPathRecorder
    {
        private const double MinSampleIntervalSec = 0.08;
        private const double MinTcpDistanceMm = 0.5;
        private const double MinJointDeltaDeg = 0.05;

        private readonly List<TeachingPathSample> samples = new();
        private double startTimeSec;
        private double lastSampleTimeSec;

        public bool IsRecording { get; private set; }
        public int SampleCount => samples.Count;

        public void Start(double nowSec)
        {
            samples.Clear();
            IsRecording = true;
            startTimeSec = nowSec;
            lastSampleTimeSec = double.NegativeInfinity;
        }

        public bool Capture(FairinoRobotState state, double nowSec, bool force = false)
        {
            if (!IsRecording && !force)
            {
                return false;
            }

            if (state.JointPosDeg == null || state.JointPosDeg.Length < 6 || state.TcpPose == null || state.TcpPose.Length < 6)
            {
                return false;
            }

            if (!force && samples.Count > 0 && nowSec - lastSampleTimeSec < MinSampleIntervalSec)
            {
                return false;
            }

            if (!force && samples.Count > 0 && !HasMeaningfulDelta(samples[^1], state))
            {
                return false;
            }

            samples.Add(new TeachingPathSample(
                System.Math.Max(0.0, nowSec - startTimeSec),
                CopyFirstSix(state.JointPosDeg),
                CopyFirstSix(state.TcpPose)));
            lastSampleTimeSec = nowSec;
            return true;
        }

        public void Stop()
        {
            IsRecording = false;
        }

        public WaypointSequence BuildSequence(string sequenceName, string speedPreset = "slow")
        {
            var waypoints = new Waypoint[samples.Count];
            for (var index = 0; index < samples.Count; index++)
            {
                var sample = samples[index];
                var dwellSec = index + 1 < samples.Count
                    ? System.Math.Max(0.0, samples[index + 1].ElapsedSec - sample.ElapsedSec)
                    : 0.0;
                waypoints[index] = new Waypoint
                {
                    name = $"REC_{index + 1:000}",
                    jointsDeg = CopyFirstSix(sample.JointsDeg),
                    tcpMm = CopyFirstSix(sample.TcpMm),
                    moveType = "MoveJ",
                    speedPreset = speedPreset,
                    dwellSec = System.Math.Min(1.0, dwellSec),
                };
            }

            return new WaypointSequence
            {
                name = string.IsNullOrWhiteSpace(sequenceName) ? "PendantV3RecordedPath" : sequenceName.Trim(),
                created = DateTime.Now.ToString("O"),
                waypoints = waypoints,
            };
        }

        public string ToDebugSummary()
        {
            var last = samples.Count > 0 ? samples[^1] : null;
            return last == null
                ? $"recording={IsRecording}; samples=0"
                : $"recording={IsRecording}; samples={samples.Count}; lastTcp=[{Format(last.TcpMm)}]; lastJ=[{Format(last.JointsDeg)}]";
        }

        private static bool HasMeaningfulDelta(TeachingPathSample previous, FairinoRobotState state)
        {
            var tcpDistance = System.Math.Sqrt(
                System.Math.Pow(previous.TcpMm[0] - state.TcpPose[0], 2.0)
                + System.Math.Pow(previous.TcpMm[1] - state.TcpPose[1], 2.0)
                + System.Math.Pow(previous.TcpMm[2] - state.TcpPose[2], 2.0));
            if (tcpDistance >= MinTcpDistanceMm)
            {
                return true;
            }

            for (var index = 0; index < 6; index++)
            {
                if (System.Math.Abs(previous.JointsDeg[index] - state.JointPosDeg[index]) >= MinJointDeltaDeg)
                {
                    return true;
                }
            }

            return false;
        }

        private static double[] CopyFirstSix(double[] values)
        {
            var copy = new double[6];
            if (values != null)
            {
                Array.Copy(values, copy, System.Math.Min(6, values.Length));
            }

            return copy;
        }

        private static string Format(double[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            var formatted = new string[System.Math.Min(6, values.Length)];
            for (var index = 0; index < formatted.Length; index++)
            {
                formatted[index] = values[index].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            }

            return string.Join(",", formatted);
        }

        private sealed class TeachingPathSample
        {
            public TeachingPathSample(double elapsedSec, double[] jointsDeg, double[] tcpMm)
            {
                ElapsedSec = elapsedSec;
                JointsDeg = jointsDeg;
                TcpMm = tcpMm;
            }

            public double ElapsedSec { get; }
            public double[] JointsDeg { get; }
            public double[] TcpMm { get; }
        }
    }
}
