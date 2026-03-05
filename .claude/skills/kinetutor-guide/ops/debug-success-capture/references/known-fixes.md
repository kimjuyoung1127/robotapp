# Known Fixes (KineTutor3D)

## A) PlayMode가 실행되지만 테스트가 잡히지 않음
- 증상: PlayMode `summary.total=0` 또는 테스트 미탐지
- 조치:
  1. `Assets/Tests/PlayMode/*.asmdef` 확인
  2. 테스트 파일이 해당 asmdef 범위에 포함되는지 확인
  3. 필요 시 `overrideReferences + nunit.framework.dll` 조합 사용

## B) Input 관련 InvalidOperationException
- 증상: `You are trying to read Input using the UnityEngine.Input class...`
- 조치:
  1. `EventSystem`에서 `StandaloneInputModule` 제거
  2. `InputSystemUIInputModule` 추가
  3. 테스트/툴팁 경로에서 입력 의존 코드 최소화 또는 가드 추가

## C) TooltipRoot 비활성 판정 불안정
- 증상: 툴팁 숨김 후에도 테스트에서 활성처럼 보임
- 조치:
  1. `TooltipRoot`를 `RectTransform` 기반 UI 오브젝트로 유지
  2. 테스트는 활성 계층 기준 탐색(`GameObject.Find`)과 fallback 탐색을 분리

## D) Step 가시성 테스트 오탐
- 증상: 패널 전환 직후 상태 불일치
- 조치:
  1. 애니메이션 지속시간을 고려한 대기(`WaitForSecondsRealtime`) 추가
  2. Step 전환 후 프레임/시간 대기 뒤 assertion 수행
