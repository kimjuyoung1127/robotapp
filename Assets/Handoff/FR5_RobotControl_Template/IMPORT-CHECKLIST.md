# IMPORT CHECKLIST

## 1. 기본 확인

- `RobotControl.unity`가 열리는지 확인
- 콘솔 compile error가 없는지 확인
- `FAIRINO_FR5_Control.prefab`가 Resources에서 로드되는지 확인

## 2. Mock 기본 동작

- Play 진입 시 Mock 기본 시작 확인
- `ConnectionPanel` 표시 확인
- `JointControlPanel` 6축 슬라이더 표시 확인
- `TcpControlPanel` 표시 확인
- `StatePanel` 표시 확인
- `Why It Moved` 표시 확인

## 3. 조작 검증

- 슬라이더 6축 이동 확인
- `Home`, `Ready`, `Folded`, `Current` 프리셋 확인
- TCP 입력값 변경 시 preview 반영 확인
- `Diagnostics Drawer` 열기/닫기 확인
- 기즈모 토글 / trail clear 확인

## 4. 3D 검증

- 3D FR5 control prefab이 보이는지 확인
- joint 값 변경 시 3D 관절 미러가 되는지 확인
- frame gizmo / trail / displacement arrow가 보이는지 확인

## 5. Teaching / Playback

- waypoint 저장
- play
- loop
- stop
- export / import 버튼 노출 확인

## 6. 비포함 기능

아래는 이 번들 범위 밖입니다.

- 실제 로봇 연결
- 실제 SDK DLL
- 실제 `Enable`
- 실제 `MoveJ`
