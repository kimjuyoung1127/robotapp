// Folder: App - External hand-tracking input payloads and adapters for RobotControl.
using System;
using UnityEngine;

namespace KineTutor3D.App.HandTracking
{
    /// <summary>
    /// 폰 또는 외부 추적기가 전송하는 최소 손 입력 payload입니다.
    /// 1차 파일럿은 landmark 전체 대신 정규화된 축약 control 값만 다룹니다.
    /// </summary>
    [Serializable]
    public sealed class HandPoseSample
    {
        public int seq;
        public bool tracked;
        public float handX;
        public float handY;
        public float pinch;
        public float palmYaw;
        public float palmPitch;
        public float confidence = 1f;
        public long sentUnixMs;
        public string sourceId = "phone";

        /// <summary>
        /// JSON 텍스트를 샘플로 역직렬화하고 기본 clamp를 적용합니다.
        /// </summary>
        public static bool TryParse(string json, out HandPoseSample sample)
        {
            sample = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                sample = JsonUtility.FromJson<HandPoseSample>(json);
                if (sample == null)
                {
                    return false;
                }

                sample.Sanitize();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// 수신값의 범위를 기본 teaching-friendly 값으로 정리합니다.
        /// </summary>
        public void Sanitize()
        {
            handX = Mathf.Clamp(handX, -1f, 1f);
            handY = Mathf.Clamp(handY, -1f, 1f);
            pinch = Mathf.Clamp01(pinch);
            confidence = Mathf.Clamp01(confidence);
            palmYaw = Mathf.Clamp(palmYaw, -180f, 180f);
            palmPitch = Mathf.Clamp(palmPitch, -180f, 180f);
        }

        /// <summary>
        /// 디버그용 예시 payload를 생성합니다.
        /// </summary>
        public static HandPoseSample CreateDebugPayload()
        {
            return new HandPoseSample
            {
                seq = 1,
                tracked = true,
                handX = 0.12f,
                handY = -0.34f,
                pinch = 0.77f,
                palmYaw = 18f,
                palmPitch = -9f,
                confidence = 0.95f,
                sentUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                sourceId = "debug-phone"
            };
        }

        /// <summary>
        /// 텍스트 파일이나 디버그 로그에 붙일 수 있는 JSON 포맷을 반환합니다.
        /// </summary>
        public string ToPrettyJson()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}
