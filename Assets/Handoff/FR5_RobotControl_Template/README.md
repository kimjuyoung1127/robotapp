# FR5 RobotControl Template

이 번들은 **Mock-only FR5 RobotControl template**입니다.

## 포함 기능

- 6축 조인트 슬라이더
- `Home`, `Ready`, `Folded`, `Current` 프리셋
- TCP 입력/미리보기
- `State` 패널
- `Why It Moved`
- `Diagnostics Drawer`
- 3D FR5 control prefab 미러
- teaching / playback

## 포함하지 않는 기능

- 실제 로봇 `Live Connect`
- 실제 컨트롤러 `GetVersion`
- 실제 상태 `ReadState`
- 실기 `Enable`
- 실기 `MoveJ`

## 성격

- 이 번들은 동료가 FR5 RobotControl 구조를 재사용하거나 다른 로봇 제어 화면의 출발점으로 삼기 위한 전달용 템플릿입니다.
- 기본 동작은 `Mock` 기준입니다.
- Live SDK DLL은 포함하지 않습니다.

## 필수 전제

- Unity Robotics URDF Importer 패키지가 있어야 합니다.
- import 후 `RobotControl.unity`를 열어 direct play 기준으로 검증합니다.
- `Resources/Robots/FAIRINO_FR5.prefab`
- `Resources/Robots/FAIRINO_FR5_Control.prefab`
- `Resources/LearningTabs/FAIRINO_FR5.json`
  경로는 유지하는 것을 권장합니다.

## 권장 사용 순서

1. 새 프로젝트에 `.unitypackage`를 import합니다.
2. `DEPENDENCIES.md`를 보고 누락된 패키지/공용 타입이 없는지 확인합니다.
3. `RobotControl.unity`를 열고 Mock 기준으로 실행합니다.
4. `IMPORT-CHECKLIST.md` 순서로 기능을 검증합니다.
