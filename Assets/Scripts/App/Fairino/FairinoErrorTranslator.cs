// Folder: App - Application controllers and services; single UnityEngine entry point.
using System.Collections.Generic;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO 에러 코드를 사용자 친화적 메시지로 변환합니다.
    /// JSON 설정의 errorMessages 매핑을 사용합니다.
    /// </summary>
    public sealed class FairinoErrorTranslator
    {
        private readonly Dictionary<int, string> messages = new Dictionary<int, string>();

        /// <summary>
        /// 기본 에러 메시지 매핑으로 번역기를 생성합니다.
        /// </summary>
        public FairinoErrorTranslator()
        {
            messages[0]  = "OK";
            messages[-1] = "연결 실패: IP 주소 또는 네트워크 상태를 확인하세요.";
            messages[-2] = "로봇 비활성 상태입니다. Enable 버튼을 눌러주세요.";
            messages[-3] = "관절 제한 초과: 목표 각도가 허용 범위를 벗어났습니다.";
            messages[-4] = "비상 정지 활성 상태입니다. 해제 후 재시도하세요.";
            messages[-5] = "모션 충돌 감지: 이전 동작이 완료되지 않았습니다.";
            messages[-6] = "통신 타임아웃: 네트워크 연결을 확인하세요.";
            messages[-7] = "자동 모드 전환에 실패했습니다. 수동/티칭 상태를 확인하세요.";
            messages[-8] = "드래그 티칭이 활성화되어 있습니다. 종료 후 재시도하세요.";
            messages[-9] = "컨트롤러 fault가 남아 있습니다. Reset Error 후 다시 시도하세요.";
        }

        /// <summary>
        /// JSON 설정에서 에러 메시지를 추가합니다.
        /// </summary>
        public void AddMapping(int errorCode, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                messages[errorCode] = message;
            }
        }

        /// <summary>
        /// 에러 코드를 사용자 친화적 메시지로 변환합니다.
        /// </summary>
        public string Translate(int errorCode)
        {
            if (messages.TryGetValue(errorCode, out var msg))
            {
                return msg;
            }

            return $"알 수 없는 에러 (코드: {errorCode})";
        }

        /// <summary>
        /// 에러 코드로 FairinoResult를 생성합니다.
        /// </summary>
        public FairinoResult ToResult(int errorCode)
        {
            return new FairinoResult(errorCode, Translate(errorCode));
        }
    }
}
