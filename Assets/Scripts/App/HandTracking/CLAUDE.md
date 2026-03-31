# App/HandTracking/

외부 손 추적 입력을 RobotControl 런타임에 연결하는 어댑터 폴더입니다.

## 주요 역할
- `HandPoseSample.cs` — 손 자세 샘플 데이터
- `IHandPoseSource.cs` — 손 추적 입력 추상화
- `UdpHandPoseReceiver.cs` — UDP 수신 구현

## 규칙
1. 외부 입력 포맷 변환만 담당하고 UI나 기구학 계산은 포함하지 않음
2. 런타임에서 교체 가능한 입력 소스로 유지
